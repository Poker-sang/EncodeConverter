using System.Diagnostics;
using System.Text;

namespace EncodeConverter.Misc;

[DebuggerDisplay($"{{{nameof(CodePage)}}}: {{{nameof(DisplayName)}}}")]
public record EncodingItem(
    int CodePage,
    string DisplayName,
    string Name)
{
    public EncodingItem(EncodingInfo info) : this(info.CodePage, info.DisplayName, info.Name)
    {
    }

    public EncodingItem(Encoding encoding) : this(encoding.CodePage, encoding.EncodingName, encoding.BodyName)
    {
    }

    public bool IsPinned { get; set; }

    public override int GetHashCode() => CodePage;
}
