using System;
using System.Collections.Generic;
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

    private FileInfo? FileInfo { get; }

    public string? Path => FileInfo?.DirectoryName;

    public byte[]? NameBytes { get; }

    public string? Name => FileInfo?.Name;

    public byte[]? ContentBytes { get; }

    public string? Content { get; }

    [ObservableProperty] private BitmapImage? _thumbnail;

    public List<EncodingItem>? DetectionDetails { get; }

    private EncodingItem? _sourceEncoding;

    public EncodingItem? SourceEncoding
    {
        get => _sourceEncoding;
        set
        {
            if (Equals(value, _sourceEncoding))
                return;
            _sourceEncoding = value;
            if (_sourceEncoding is not null && IsStorageFileNotNull)
            {
                SourceEncodingName = Encoding.GetEncoding(_sourceEncoding.CodePage).GetString(NameBytes);
                SourceEncodingContent = Encoding.GetEncoding(_sourceEncoding.CodePage).GetString(ContentBytes);
            }
            else
            {
                SourceEncodingName = "";
                SourceEncodingContent = "";
            }
            OnPropertyChanged();
        }
    }

    public string SourceEncodingName = "";

    public string SourceEncodingContent = "";

    [ObservableProperty] private EncodingItem? _destinationEncoding;

    [ObservableProperty] private bool _parseName = true;

    [ObservableProperty] private bool _parseContent = true;

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
    }

#pragma warning disable CA1822 // 将成员标记为 static
    public List<EncodingItem> Encodings => NativeHelper.EncodingList;
#pragma warning restore CA1822 // 将成员标记为 static
}

