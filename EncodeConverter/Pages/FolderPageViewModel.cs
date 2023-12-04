using System.IO;
using System.Text;
using Windows.Storage;

namespace EncodeConverter.Pages;

public class FolderPageViewModel(StorageFolder? item) : StorageItemPageViewModel<StorageFolder, DirectoryInfo>(item)
{
    protected override int SourceOriginalEncoding
    {
        get => AppSetting.FolderOriginalEncodingCodePage;
        set => AppSetting.FolderOriginalEncodingCodePage = value;
    }

    protected override int SourceDestinationEncoding
    {
        get => AppSetting.FolderDestinationEncodingCodePage;
        set => AppSetting.FolderDestinationEncodingCodePage = value;
    }

    protected override bool SourceKeepOriginal
    {
        get => false;
        set { }
    }

    protected override bool SourceTranscodeName
    {
        get => AppSetting.FolderTranscodeName;
        set => AppSetting.FolderTranscodeName = value;
    }

    protected override bool SourceTranscodeContent
    {
        get => AppSetting.FolderTranscodeChildren;
        set => AppSetting.FolderTranscodeChildren = value;
    }

    protected override bool SourceDestinationEncodingUseSystem
    {
        get => AppSetting.FolderDestinationEncodingUseSystem;
        set => AppSetting.FolderDestinationEncodingUseSystem = value;
    }

    protected override (byte[], string) SetContent(DirectoryInfo info)
    {
        var sb = new StringBuilder("");
        foreach (var fileSystemInfo in info.EnumerateFileSystemInfos())
        {
            sb = sb.Append(fileSystemInfo.Name).Append(' ');
            if (sb.Length >= ContentMaxLength)
                break;
        }
        var str = sb.ToString(0, 64);
        var bytes = EncodingHelper.SystemEncoding.GetBytes(str);
        return (bytes, str);
    }
}
