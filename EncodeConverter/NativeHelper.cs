using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace EncodeConverter;

public static class NativeHelper
{
    static NativeHelper()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        SystemEncoding = Encoding.GetEncoding(0);
        EncodingList = Encoding.GetEncodings().Select(t => new EncodingItem(t)).ToList();
        EncodingList.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal));
        var list = JsonSerializer.Deserialize<List<int>>(AppContext.AppSetting.PinnedEncodings) ?? [];
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

    public static void Initialize()
    {
    }

    public static Encoding SystemEncoding { get; }

    public static List<EncodingItem> EncodingList { get; }
}
