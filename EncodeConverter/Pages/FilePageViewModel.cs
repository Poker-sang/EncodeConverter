using System.IO;
using Windows.Storage;
using EncodeConverter.Misc;

namespace EncodeConverter.Pages;

public class FilePageViewModel(StorageFile? file) : StorageItemPageViewModel<StorageFile, FileInfo>(file)
{
    protected bool SourceKeepOriginal
    {
        get => AppSetting.FileKeepOriginal;
        set => AppSetting.FileKeepOriginal = value;
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

