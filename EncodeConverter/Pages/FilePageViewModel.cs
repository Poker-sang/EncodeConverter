using System.IO;
using Windows.Storage;
using EncodeConverter.Misc;

namespace EncodeConverter.Pages;

public class FilePageViewModel(StorageFile? file) : StorageItemPageViewModel<StorageFile, FileInfo>(file)
{
    protected override int SourceOriginalEncodingCodePage
    {
        get => AppSetting.FileOriginalEncodingCodePage;
        set => AppSetting.FileOriginalEncodingCodePage = value;
    }

    protected override int SourceDestinationEncodingCodePage
    {
        get => AppSetting.FileDestinationEncodingCodePage;
        set => AppSetting.FileDestinationEncodingCodePage = value;
    }

    protected override bool SourceKeepOriginal
    {
        get => AppSetting.FileKeepOriginal;
        set => AppSetting.FileKeepOriginal = value;
    }

    protected override bool SourceTranscodeName
    {
        get => AppSetting.FileTranscodeName;
        set => AppSetting.FileTranscodeName = value;
    }

    protected override bool SourceTranscodeContent
    {
        get => AppSetting.FileTranscodeContent;
        set => AppSetting.FileTranscodeContent = value;
    }

    protected override bool SourceDestinationEncodingUseSystem
    {
        get => AppSetting.FileDestinationEncodingUseSystem;
        set => AppSetting.FileDestinationEncodingUseSystem = value;
    }

    protected override (byte[], string) SetContent(FileInfo info)
    {
        using var fileStream = info.OpenRead();
        var bytes = new byte[ContentMaxLength];
        _ = fileStream.Read(bytes, 0, ContentMaxLength);
        fileStream.Position = 0; // reset
        var str = EncodingHelper.SystemEncoding.GetString(bytes);
        return (bytes, str);
    }
}

