using WinUI3Utilities.Attributes;

namespace EncodeConverter;

[GenerateConstructor]
public partial record AppSettings
{
    public bool ParseName { get; set; } = true;

    public bool ParseContent { get; set; } = true;

    public int SourceEncodingCodePage { get; set; } = 932;

    public int DestinationEncodingCodePage { get; set; } = 936;

    public string PinnedEncodings { get; set; } = "[936, 932, 950, 65001, 437]";

    public bool KeepOriginalFile { get; set; } = true;

    public AppSettings()
    {
        
    }
}
