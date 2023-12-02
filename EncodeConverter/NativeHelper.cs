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
        EncodingList = Encoding.GetEncodings().Select(t => new EncodingItem(t)).ToList();
        EncodingList.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal));
        var list = new List<int> { 936, 932, 950, 65001, 437 };
        list.Reverse();
        foreach (var i in list)
        {
            if (EncodingList.Find(t => t.CodePage == i) is { } encodingInfo)
            {
                _ = EncodingList.Remove(encodingInfo);
                EncodingList.Insert(0, encodingInfo);
                encodingInfo.IsPinned = true;
            }
        }

    }

    public static Encoding SystemEncoding { get; }

    public static List<EncodingItem> EncodingList { get; }
}
