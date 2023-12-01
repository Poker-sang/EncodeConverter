using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EncodeConverter;

public class NativeHelper
{
    static NativeHelper()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        SystemEncoding = Encoding.GetEncoding(0);
        EncodingList = [.. Encoding.GetEncodings()];
        EncodingList.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal));
    }

    public static Encoding SystemEncoding { get; }

    public static List<EncodingInfo> EncodingList { get; }
}
