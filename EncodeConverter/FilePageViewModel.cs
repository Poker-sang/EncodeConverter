using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

    private FileInfo? FileInfo { get; }

    public string? Path => FileInfo?.DirectoryName;

    public byte[]? NameBytes { get; }

    public string? Name => FileInfo?.Name;

    public byte[]? ContentBytes { get; }

    public string? Content { get; }

    [ObservableProperty] private BitmapImage? _thumbnail;

    public List<EncodingItem>? DetectionDetails { get; }

    public string SourceEncodingName = "";

    public string SourceEncodingContent = "";

    public EncodingItem SourceEncoding
    {
        get => Encodings.Find(t => t.CodePage == AppSetting.SourceEncodingCodePage)!;
        set
        {
            if (value.CodePage == AppSetting.SourceEncodingCodePage)
                return;
            AppSetting.SourceEncodingCodePage = value.CodePage;
            AppContext.SaveConfiguration(AppSetting);
            SetSourceEncoding();
            OnPropertyChanged();
        }
    }

    public EncodingItem DestinationEncoding
    {
        get => Encodings.Find(t => t.CodePage == AppSetting.DestinationEncodingCodePage)!;
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

    public bool ParseName
    {
        get => AppSetting.ParseName;
        set
        {
            if (value == AppSetting.ParseName)
                return;
            AppSetting.ParseName = value;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
        }
    }

    public bool ParseContent
    {
        get => AppSetting.ParseContent;
        set
        {
            if (value == AppSetting.ParseContent)
                return;
            AppSetting.ParseContent = value;
            AppContext.SaveConfiguration(AppSetting);
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
            .Where(d => d.Confidence > 0.5)
            .OrderBy(x => x.Confidence)
            .Select(d => Encodings.Find(x => x.DisplayName == d.Encoding.EncodingName)!)
            .ToList();
        SetSourceEncoding();
    }

    private void SetSourceEncoding()
    {
        if (IsStorageFileNotNull)
        {
            SourceEncodingName = Encoding.GetEncoding(SourceEncoding.CodePage).GetString(NameBytes);
            SourceEncodingContent = Encoding.GetEncoding(SourceEncoding.CodePage).GetString(ContentBytes);
        }
        else
        {
            SourceEncodingName = "";
            SourceEncodingContent = "";
        }
    }

#pragma warning disable CA1822 // 将成员标记为 static
    public List<EncodingItem> Encodings => NativeHelper.EncodingList;

    public AppSettings AppSetting => AppContext.AppSetting;
#pragma warning restore CA1822 // 将成员标记为 static
}

