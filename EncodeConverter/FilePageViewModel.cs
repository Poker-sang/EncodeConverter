using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Storage;
using UtfUnknown;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EncodeConverter;

public partial class FilePageViewModel : ObservableObject
{
    public StorageFile? StorageFile { get; }

    private FileInfo? FileInfo { get; }

    public string? Path => FileInfo?.DirectoryName;

    public string? Name => FileInfo?.Name;

    public byte[]? Content { get; }

    public string? ContentPreview { get; }

    public List<EncodingInfo>? DetectionDetails { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableEncodings))]
    private EncodingInfo? _sourceEncoding;

    [ObservableProperty] private EncodingInfo? _destinationEncoding;

    public FilePageViewModel(StorageFile? file = null)
    {
        _current = this;
        StorageFile = file;
        if (file is null)
            return;
        FileInfo = new(file.Path);
        using var fileStream = FileInfo.OpenRead();
        Content = new byte[64];
        _ = fileStream.Read(Content, 0, Content.Length);
        fileStream.Position = 0; // reset
        ContentPreview = NativeHelper.SystemEncoding.GetString(Content);
        var result = CharsetDetector.DetectFromStream(fileStream);
        DetectionDetails = result.Details.Where(d => d.Confidence > 0.5).Select(d =>
        {
            var i = Encodings.BinarySearch(new(CodePagesEncodingProvider.Instance, 0, "", d.Encoding.EncodingName), new EncodingComparer());
            return Encodings[i];
        }).ToList();
    }

    private class EncodingComparer : IComparer<EncodingInfo>
    {
        public int Compare(EncodingInfo? x, EncodingInfo? y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (y is null)
                return 1;
            if (x is null)
                return -1;
            return string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);
        }
    }

    private static FilePageViewModel? _current;

    public static string? GetPreviewSourceString(int codePage)
    {
        if (_current is not { StorageFile: not null })
            return null;
        var srcEncoding = Encoding.GetEncoding(codePage);
        return srcEncoding.GetString(_current.Content!);
    }

    public static string? GetPreviewDestinationString(int codePage)
    {
        if (_current is not { SourceEncoding: not null, StorageFile: not null })
            return null;
        var srcEncoding = Encoding.GetEncoding(_current.SourceEncoding.CodePage);
        var dstEncoding = Encoding.GetEncoding(codePage);
        var s = srcEncoding.GetString(_current.Content!);
        var bytes = dstEncoding.GetBytes(s);
        return NativeHelper.SystemEncoding.GetString(bytes);
    }

    public List<EncodingInfo> Encodings => NativeHelper.EncodingList;

    public List<EncodingInfo> UpdatableEncodings => NativeHelper.EncodingList.ToList();
}

