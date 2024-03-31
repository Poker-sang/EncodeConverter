using System.Collections.Generic;
using EncodeConverter.Misc;
using WinUI3Utilities.Attributes;

namespace EncodeConverter;

[GenerateConstructor]
public partial record AppSettings
{
    public List<int> PinnedEncodings { get; set; } = [936, 932, 950, 65001, 437];

    #region FilePage

    public bool FileTranscodeName { get; set; } = true;

    public bool FileTranscodeContent { get; set; } = true;

    public int FileOriginalEncodingCodePage { get; set; } = 932;

    public int FileDestinationEncodingCodePage { get; set; } = 0;

    public bool FileDestinationEncodingUseSystem { get; set; } = false;

    public bool FileKeepOriginal { get; set; } = true;

    #endregion

    #region FolderPage

    public bool FolderTranscodeName { get; set; } = true;

    public bool FolderTranscodeChildren { get; set; } = true;

    public int FolderOriginalEncodingCodePage { get; set; } = 932;

    public int FolderDestinationEncodingCodePage { get; set; } = 0;

    public bool FolderDestinationEncodingUseSystem { get; set; } = false;

    public FileFilterType FolderFileFilter { get; set; }

    public bool FolderFilterExtensionsCaseSensitive { get; set; } = false;

    public string FolderFilterExtensions { get; set; } = "txt;md;";

    public string FolderFilterRegex { get; set; } = "";

    #endregion

    #region FolderPage

    public int TextOriginalEncodingCodePage { get; set; } = 932;

    public int TextDestinationEncodingCodePage { get; set; } = 0;

    public bool TextDestinationEncodingUseSystem { get; set; } = false;

    // public bool TextDoNotTranscode { get; set; } = false;

    #endregion

    public AppSettings()
    {

    }
}
