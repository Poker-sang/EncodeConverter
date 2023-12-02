using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Storage;
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

    public List<EncodingInfo>? DetectionDetails { get; }

    private EncodingInfo? _sourceEncoding;

    public EncodingInfo? SourceEncoding
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

    [ObservableProperty] private EncodingInfo? _destinationEncoding;

    [ObservableProperty] private bool _parseName;

    [ObservableProperty] private bool _parseContent;

    public FilePageViewModel(StorageFile? file = null)
    {
        StorageFile = file;
        if (file is null)
            return;
        FileInfo = new(file.Path);
        NameBytes = NativeHelper.SystemEncoding.GetBytes(Name!);
        using var fileStream = FileInfo.OpenRead();
        ContentBytes = new byte[64];
        _ = fileStream.Read(ContentBytes, 0, ContentBytes.Length);
        fileStream.Position = 0; // reset
        Content = NativeHelper.SystemEncoding.GetString(ContentBytes);
        var result = CharsetDetector.DetectFromStream(fileStream);
        DetectionDetails = result.Details.Where(d => d.Confidence > 0.5).Select(d =>
        {
            var i = Encodings.FindIndex(x => x.DisplayName == d.Encoding.EncodingName);
            return Encodings[i];
        }).ToList();
    }

    public List<EncodingInfo> Encodings => NativeHelper.EncodingList;
}

