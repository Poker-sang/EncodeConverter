using WinUI3Utilities.Attributes;

namespace EncodeConverter;

[GenerateConstructor]
public partial record AppSettings
{
    public bool TranscodeName { get; set; } = true;

    public bool TranscodeContent { get; set; } = true;

    public int OriginalEncodingCodePage { get; set; } = 932;

    public int DestinationEncodingCodePage { get; set; } = 936;

    public bool DestinationEncodingUseSystem { get; set; } = false;

    public string PinnedEncodings { get; set; } = "[936, 932, 950, 65001, 437]";

    public bool KeepOriginalFile { get; set; } = true;

    public AppSettings()
    {
        
    }
}
