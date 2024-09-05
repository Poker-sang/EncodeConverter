using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using EncodeConverter.Misc;
using Microsoft.UI.Xaml.Media.Imaging;
using UtfUnknown;
using Windows.Storage;
using Windows.Storage.FileProperties;
using WinUI3Utilities;

namespace EncodeConverter.Pages;

public abstract partial class StorageItemPageViewModel<T, TInfo> : AbstractViewModel, IStorageItemPageViewModel where T : IStorageItem where TInfo : FileSystemInfo
{
    protected const int ContentMaxLength = 64;

    [MemberNotNullWhen(true, nameof(StorageItem), nameof(Info), nameof(Path), nameof(NameBytes), nameof(Name), nameof(ContentBytes), nameof(Content))]
    public bool IsStorageItemNotNull => StorageItem is not null;

    public T? StorageItem { get; }

    public TInfo? Info { get; }

    public string? Path => System.IO.Path.GetDirectoryName(Info?.FullName);

    public byte[]? NameBytes { get; }

    public string? Name { get; }

    public byte[]? ContentBytes { get; protected set; }

    public string? Content { get; protected set; }

    public string OriginalEncodingName { get; set; } = "";

    public string OriginalEncodingContent { get; set; } = "";

    [ObservableProperty] private BitmapImage? _thumbnail;

    protected StorageItemPageViewModel(T? item)
    {
        StorageItem = item;
        switch (item)
        {
            case StorageFile file:
                Info = new FileInfo(item.Path) as TInfo;
                _ = file.GetThumbnailAsync(ThumbnailMode.SingleItem, 72)
                    .AsTask().ContinueWith(task =>
                    {
                        var stream = task.Result;
                        var bitmap = new BitmapImage();
                        bitmap.SetSource(stream);
                        Thumbnail = bitmap;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                break;
            case StorageFolder folder:
                Info = new DirectoryInfo(item.Path) as TInfo;
                _ = folder.GetThumbnailAsync(ThumbnailMode.SingleItem, 72)
                    .AsTask().ContinueWith(task =>
                    {
                        var stream = task.Result;
                        var bitmap = new BitmapImage();
                        bitmap.SetSource(stream);
                        Thumbnail = bitmap;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                break;
            case null:
                return;
            default:
                ThrowHelper.Argument(item, "Invalid type.");
                break;
        }

        Name = Info!.Name;
        NameBytes = EncodingHelper.SystemEncoding.GetBytes(Name);

        // ReSharper disable once VirtualMemberCallInConstructor
        (ContentBytes, Content) = SetContent(Info);

        var result = CharsetDetector.DetectFromBytes(ContentBytes);
        DetectionDetails = result.Details
            .Where(d => d.Confidence >= 0.5)
            .OrderByDescending(x => x.Confidence)
            .Select(d => EncodingHelper.GetEncodingItemOrFetch(d.Encoding.CodePage))
            .ToList();
        OnOriginalEncodingChanged();
    }

    protected abstract (byte[], string) SetContent(TInfo info);

    protected sealed override void OnOriginalEncodingChanged()
    {
        if (IsStorageItemNotNull)
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

    public bool TranscodeName
    {
        get => SourceTranscodeName;
        set
        {
            if (value == SourceTranscodeName)
                return;
            SourceTranscodeName = value;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
        }
    }

    public bool TranscodeContent
    {
        get => SourceTranscodeContent;
        set
        {
            if (value == SourceTranscodeContent)
                return;
            SourceTranscodeContent = value;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
        }
    }

    protected bool SourceTranscodeName
    {
        get => AppSetting.FileTranscodeName;
        set => AppSetting.FileTranscodeName = value;
    }

    protected bool SourceTranscodeContent
    {
        get => AppSetting.FileTranscodeContent;
        set => AppSetting.FileTranscodeContent = value;
    }
}
