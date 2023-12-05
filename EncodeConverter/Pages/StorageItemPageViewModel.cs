using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using UtfUnknown;

namespace EncodeConverter.Pages;

public interface IStorageItemPageViewModel : INotifyPropertyChanged
{
    [MemberNotNullWhen(true, nameof(Path), nameof(NameBytes), nameof(Name), nameof(ContentBytes), nameof(Content), nameof(DetectionDetails))]
    bool IsStorageItemNotNull { get; }

    string? Path { get; }

    byte[]? NameBytes { get; }

    string? Name { get; }

    byte[]? ContentBytes { get; }

    string? Content { get; }

    string OriginalEncodingName { get; set; }

    string OriginalEncodingContent { get; set; }

    EncodingItem OriginalEncoding { get; set; }

    EncodingItem DestinationEncoding { get; set; }

    bool KeepOriginal { get; set; }

    bool TranscodeName { get; set; }

    bool TranscodeContent { get; set; }

    bool DestinationEncodingUseSystem { get; set; }

    List<EncodingItem>? DetectionDetails { get; }
}

public abstract class AbstractViewModel : ObservableObject
{
    public List<EncodingItem>? DetectionDetails { get; protected set; }

    public EncodingItem OriginalEncoding
    {
        get => EncodingHelper.GetEncodingItemOrFetch(SourceOriginalEncodingCodePage);
        set
        {
            if (value.CodePage == SourceOriginalEncodingCodePage)
                return;
            SourceOriginalEncodingCodePage = value.CodePage;
            AppContext.SaveConfiguration(AppSetting);
            OnOriginalEncodingChanged();
            OnPropertyChanged();
        }
    }

    protected abstract void OnOriginalEncodingChanged();

    public EncodingItem DestinationEncoding
    {
        get => EncodingHelper.GetEncodingItemOrFetch(SourceDestinationEncodingCodePage);
        set
        {
            if (value.CodePage == SourceDestinationEncodingCodePage)
                return;
            SourceDestinationEncodingCodePage = value.CodePage;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
        }
    }

    public bool DestinationEncodingUseSystem
    {
        get => SourceDestinationEncodingUseSystem;
        set
        {
            if (value == SourceDestinationEncodingUseSystem)
                return;
            SourceDestinationEncodingUseSystem = value;
            AppContext.SaveConfiguration(AppSetting);
            DestinationEncoding = EncodingHelper.SystemEncodingInfo;
            OnPropertyChanged();
        }
    }

    protected abstract int SourceOriginalEncodingCodePage { get; set; }

    protected abstract int SourceDestinationEncodingCodePage { get; set; }

    protected abstract bool SourceDestinationEncodingUseSystem { get; set; }

#pragma warning disable CA1822 // 将成员标记为 static
    public ObservableCollection<EncodingItem> Encodings => EncodingHelper.EncodingCollection;

    public AppSettings AppSetting => AppContext.AppSettings;
#pragma warning restore CA1822 // 将成员标记为 static
}

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
        if (item is null)
            return;
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
            default:
                throw new ArgumentException("Invalid type", nameof(item));
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

    public bool KeepOriginal
    {
        get => SourceKeepOriginal;
        set
        {
            if (value == SourceKeepOriginal)
                return;
            SourceKeepOriginal = value;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
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

    protected abstract bool SourceKeepOriginal { get; set; }

    protected abstract bool SourceTranscodeName { get; set; }

    protected abstract bool SourceTranscodeContent { get; set; }
}
