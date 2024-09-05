using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using EncodeConverter.Misc;
using Windows.Storage;

namespace EncodeConverter.Pages;

public class FolderPageViewModel(StorageFolder? item) : StorageItemPageViewModel<StorageFolder, DirectoryInfo>(item)
{
    public FileFilterType FileFilter
    {
        get => AppSetting.FolderFileFilter;
        set => AppSetting.FolderFileFilter = value;
    }

    public int FileFilterIndex
    {
        get => (int)AppSetting.FolderFileFilter;
        set
        {
            var v = (FileFilterType)value;
            if (v == FileFilter)
                return;
            FileFilter = v;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
            OnPropertyChanged(nameof(FilterIsSpecifyExtensions));
            OnPropertyChanged(nameof(FilterIsUseRegex));
        }
    }

    public bool FilterIsSpecifyExtensions => FileFilter is FileFilterType.SpecifyExtensions;

    public bool FilterIsUseRegex => FileFilter is FileFilterType.UseRegex;

    public bool FilterExtensionsCaseSensitive
    {
        get => AppSetting.FolderFilterExtensionsCaseSensitive;
        set
        {
            if (value == AppSetting.FolderFilterExtensionsCaseSensitive)
                return;
            AppSetting.FolderFilterExtensionsCaseSensitive = value;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
        }
    }

    public string FilterExtensions
    {
        get => AppSetting.FolderFilterExtensions;
        set
        {
            if (value == AppSetting.FolderFilterExtensions)
                return;
            AppSetting.FolderFilterExtensions = value;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
        }
    }

    public string FilterRegex
    {
        get => AppSetting.FolderFilterRegex;
        set
        {
            if (value == AppSetting.FolderFilterRegex)
                return;
            AppSetting.FolderFilterRegex = value;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
            OnPropertyChanged(nameof(FilterRegexIsValid));
        }
    }

    public bool FilterRegexIsValid
    {
        get
        {
            try
            {
                _ = new Regex(FilterRegex);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    protected override (byte[], string) SetContent(DirectoryInfo info)
    {
        var sb = new StringBuilder();
        foreach (var fileSystemInfo in info.EnumerateFileSystemInfos())
        {
            sb = sb.Append(fileSystemInfo.Name).Append(' ');
            if (sb.Length >= ContentMaxLength)
                break;
        }
        var str = sb.Length >= ContentMaxLength ? sb.ToString(0, 64) : sb.ToString();
        var bytes = EncodingHelper.SystemEncoding.GetBytes(str);
        return (bytes, str);
    }
}
