using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Microsoft.UI.Xaml.Media.Imaging;
using UtfUnknown;

namespace EncodeConverter;

public partial class FilePageViewModel : ObservableObject
{
    [MemberNotNullWhen(true, nameof(StorageFile), nameof(FileInfo), nameof(Path), nameof(NameBytes), nameof(Name), nameof(ContentBytes), nameof(Content), nameof(DetectionDetails))]
    public bool IsStorageFileNotNull => StorageFile is not null;

    public StorageFile? StorageFile { get; }

    public FileInfo? FileInfo { get; }

    public string? Path => FileInfo?.DirectoryName;

    public byte[]? NameBytes { get; }

    public string? Name => FileInfo?.Name;

    public byte[]? ContentBytes { get; }

    public string? Content { get; }

    [ObservableProperty] private BitmapImage? _thumbnail;

    public List<EncodingItem>? DetectionDetails { get; }

    public string OriginalEncodingName = "";

    public string OriginalEncodingContent = "";

    public EncodingItem OriginalEncoding
    {
        get => NativeHelper.EncodingList.Find(t => t.CodePage == AppSetting.OriginalEncodingCodePage)!;
        set
        {
            if (value.CodePage == AppSetting.OriginalEncodingCodePage)
                return;
            AppSetting.OriginalEncodingCodePage = value.CodePage;
            AppContext.SaveConfiguration(AppSetting);
            SetOriginalEncoding();
            OnPropertyChanged();
        }
    }

    public EncodingItem DestinationEncoding
    {
        get => NativeHelper.EncodingList.Find(t => t.CodePage == AppSetting.DestinationEncodingCodePage)!;
        set
        {
            if (value.CodePage == AppSetting.DestinationEncodingCodePage)
                return;
            AppSetting.DestinationEncodingCodePage = value.CodePage;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
        }
    }

    public bool KeepOriginalFile
    {
        get => AppSetting.KeepOriginalFile;
        set
        {
            if (value == AppSetting.KeepOriginalFile)
                return;
            AppSetting.KeepOriginalFile = value;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
        }
    }

    public bool TranscodeName
    {
        get => AppSetting.TranscodeName;
        set
        {
            if (value == AppSetting.TranscodeName)
                return;
            AppSetting.TranscodeName = value;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
        }
    }

    public bool TranscodeContent
    {
        get => AppSetting.TranscodeContent;
        set
        {
            if (value == AppSetting.TranscodeContent)
                return;
            AppSetting.TranscodeContent = value;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
        }
    }

    public bool DestinationEncodingUseSystem
    {
        get => AppSetting.DestinationEncodingUseSystem;
        set
        {
            if (value == AppSetting.DestinationEncodingUseSystem)
                return;
            AppSetting.DestinationEncodingUseSystem = value;
            AppContext.SaveConfiguration(AppSetting);
            DestinationEncoding = NativeHelper.SystemEncodingInfo;
            OnPropertyChanged();
        }
    }

    public FilePageViewModel(StorageFile? file)
    {
        StorageFile = file;
        if (file is null)
            return;
        FileInfo = new(file.Path);
        NameBytes = NativeHelper.SystemEncoding.GetBytes(Name!);
        _ = file.GetThumbnailAsync(ThumbnailMode.SingleItem, 72)
            .AsTask()
            .ContinueWith(task =>
            {
                var stream = task.Result;
                var bitmap = new BitmapImage();
                bitmap.SetSource(stream);
                Thumbnail = bitmap;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        using var fileStream = FileInfo.OpenRead();
        ContentBytes = new byte[64];
        _ = fileStream.Read(ContentBytes, 0, ContentBytes.Length);
        fileStream.Position = 0; // reset
        Content = NativeHelper.SystemEncoding.GetString(ContentBytes);
        var result = CharsetDetector.DetectFromStream(fileStream);
        DetectionDetails = result.Details
            .Where(d => d.Confidence >= 0.5)
            .OrderByDescending(x => x.Confidence)
            .Select(d => NativeHelper.EncodingList.Find(x => x.CodePage == d.Encoding.CodePage) ?? NativeHelper.FetchNewEncodingItem(d.Encoding.CodePage))
            .ToList();

        SetOriginalEncoding();
    }

    private void SetOriginalEncoding()
    {
        if (IsStorageFileNotNull)
        {
            OriginalEncodingName = Encoding.GetEncoding(OriginalEncoding.CodePage).GetString(NameBytes);
            OriginalEncodingContent = Encoding.GetEncoding(OriginalEncoding.CodePage).GetString(ContentBytes);
        }
        else
        {
            OriginalEncodingName = "";
            OriginalEncodingContent = "";
        }
    }

#pragma warning disable CA1822 // 将成员标记为 static
    public ObservableCollection<EncodingItem> Encodings => NativeHelper.EncodingCollection;

    public AppSettings AppSetting => AppContext.AppSetting;
#pragma warning restore CA1822 // 将成员标记为 static
}

