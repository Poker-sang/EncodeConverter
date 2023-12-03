using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using WinUI3Utilities;

namespace EncodeConverter;

public static class NativeHelper
{
    static NativeHelper()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        SystemEncoding = Encoding.GetEncoding(0);
        EncodingCollection = Encoding.GetEncodings().Select(t => new EncodingItem(t)).ToObservableCollection();
        var field = typeof(Collection<EncodingItem>).GetField("items", BindingFlags.Instance | BindingFlags.NonPublic);
        EncodingList = field!.GetValue(EncodingCollection).To<List<EncodingItem>>();
        SystemEncodingInfo = EncodingList.Find(t => t.CodePage == SystemEncoding.CodePage)!;
        EncodingList.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal));
        var list = JsonSerializer.Deserialize<List<int>>(AppContext.AppSetting.PinnedEncodings) ?? [];
        var i = 0;
        foreach (var item in list)
        {
            if (EncodingList.Find(t => t.CodePage == item) is { } encodingInfo)
            {
                _ = EncodingList.Remove(encodingInfo);
                EncodingList.Insert(i, encodingInfo);
                encodingInfo.IsPinned = true;
                ++i;
            }
        }
    }

    public static void Initialize()
    {
    }

    public static EncodingItem? TryFetchNewEncodingItem(int codePage)
    {
        if (CodePagesEncodingProvider.Instance.GetEncoding(codePage) is not { } encoding)
            return null;
        else
        {
            var index = EncodingList.FindIndex(t => !t.IsPinned);
            if (index is -1)
                index = EncodingCollection.Count;
            var newEncodingItem = new EncodingItem(encoding);
            EncodingCollection.Insert(index, newEncodingItem);
            return newEncodingItem;
        }
    }

    public static EncodingItem FetchNewEncodingItem(int codePage)
    {
        return TryFetchNewEncodingItem(codePage) ?? throw new("Cannot fetch new encoding item.");
    }

    public static Encoding SystemEncoding { get; }

    public static EncodingItem SystemEncodingInfo { get; }

    public static ObservableCollection<EncodingItem> EncodingCollection { get; }

    public static List<EncodingItem> EncodingList { get; }
}
