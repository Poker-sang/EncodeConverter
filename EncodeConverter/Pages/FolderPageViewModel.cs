using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Storage;
using EncodeConverter.Misc;

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

    public Regex? TrgGetRegex()
    {
        try
        {
            return new(FilterRegex);
        }
        catch
        {
            return null;
        }
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
        var str = sb.Length >= ContentMaxLength ? sb.ToString(0, 64) : sb.ToString();
        var bytes = EncodingHelper.SystemEncoding.GetBytes(str);
        return (bytes, str);
    }

    protected override int SourceOriginalEncodingCodePage
    {
        get => AppSetting.FolderOriginalEncodingCodePage;
        set => AppSetting.FolderOriginalEncodingCodePage = value;
    }

    protected override int SourceDestinationEncodingCodePage
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
}
