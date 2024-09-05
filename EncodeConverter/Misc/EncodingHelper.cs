using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using WinUI3Utilities;

namespace EncodeConverter.Misc;

public static class EncodingHelper
{
    static EncodingHelper()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        SystemEncoding = Encoding.GetEncoding(0);
        EncodingCollection = [.. Encoding.GetEncodings().Select(t => new EncodingItem(t))];
        var field = typeof(Collection<EncodingItem>).GetField("items", BindingFlags.Instance | BindingFlags.NonPublic);
        EncodingList = field!.GetValue(EncodingCollection).To<List<EncodingItem>>();
        SystemEncodingInfo = EncodingList.Find(t => t.CodePage == SystemEncoding.CodePage)!;
        EncodingList.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal));
        var i = 0;
        foreach (var item in AppContext.AppSettings.PinnedEncodings)
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

        var newEncodingItem = new EncodingItem(encoding);
        var index = EncodingList.FindIndex(t => !t.IsPinned && string.Compare(t.DisplayName, newEncodingItem.DisplayName, StringComparison.Ordinal) > 0);
        if (index is -1)
            index = EncodingCollection.Count;
        EncodingCollection.Insert(index, newEncodingItem);
        return newEncodingItem;
    }

    public static EncodingItem FetchNewEncodingItem(int codePage)
    {
        return TryFetchNewEncodingItem(codePage) ?? ThrowHelper.Exception<EncodingItem>("Cannot fetch new encoding item.");
    }

    public static EncodingItem GetEncodingItemOrFetch(int codePage)
    {
        return TryGetEncodingItem(codePage) ?? FetchNewEncodingItem(codePage);
    }

    public static EncodingItem? TryGetEncodingItem(int codePage)
    {
        return codePage is 0 ? SystemEncodingInfo : EncodingList.Find(t => t.CodePage == codePage);
    }

    public static Encoding SystemEncoding { get; }

    public static EncodingItem SystemEncodingInfo { get; }

    public static ObservableCollection<EncodingItem> EncodingCollection { get; }

    public static List<EncodingItem> EncodingList { get; }
}
