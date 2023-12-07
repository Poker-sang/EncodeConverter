using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using EncodeConverter.Controls;
using EncodeConverter.Misc;

namespace EncodeConverter.Pages;

public interface IStorageItemPage
{
    event PropertyChangedEventHandler? PropertyChanged;

    IStorageItemPageViewModel Vm { get; }
}

[INotifyPropertyChanged]
public abstract partial class OriginalEncodingsPage<T> : Page where T : AbstractViewModel
{
    [ObservableProperty]
    private T _vm = null!;

    protected OriginalEncodingsPage()
    {
        Loaded += (_, _) => Initialized = true;
        Unloaded += (_, _) => Initialized = false;
    }

    public bool Initialized { get; private set; }

    protected abstract ItemsView OriginalItemsViewOverride { get; }

    protected abstract ComboBox OriginalComboBoxOverride { get; }

    protected virtual void SubscribeEvents()
    {
        OriginalItemsViewOverride.SelectionChanged += ItemsView_SelectionChanged;
        OriginalComboBoxOverride.SelectionChanged += Selector_OnSelectionChanged;
    }

    private void ItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs e)
    {
        if (sender.SelectedItem is not EncodingItem item)
            return;

        Vm.OriginalEncoding = item;
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not [EncodingItem item])
            return;

        if (Vm.DetectionDetails?.IndexOf(item) is { } i and not -1)
        {
            OriginalItemsViewOverride.Select(i);
            if (Initialized)
                OriginalItemsViewOverride.StartBringItemIntoView(i, new() { AnimationDesired = true });
        }
        else
            OriginalItemsViewOverride.DeselectAll();
    }
}

public abstract class StorageItemPage<T, TItem, TInfo> : OriginalEncodingsPage<T>, IStorageItemPage where T : StorageItemPageViewModel<TItem, TInfo> where TItem : class, IStorageItem where TInfo : FileSystemInfo
{
    IStorageItemPageViewModel IStorageItemPage.Vm => Vm;

    protected StorageItemPage()
    {
        Loaded += (_, _) => Vm.Encodings.CollectionChanged += (_, _) => PopulateLabelCollection();
    }

    protected override void SubscribeEvents()
    {
        base.SubscribeEvents();
        DestinationItemsViewOverride.SelectionChanged += ItemsView2_SelectionChanged;
        DestinationComboBoxOverride.SelectionChanged += Selector2_OnSelectionChanged;
        AnnotatedScrollBarOverride.DetailLabelRequested += AnnotatedScrollBarOverride_DetailLabelRequested;
    }

    protected abstract ItemsView DestinationItemsViewOverride { get; }

    protected abstract ComboBox DestinationComboBoxOverride { get; }

    protected abstract AnnotatedScrollBar AnnotatedScrollBarOverride { get; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        SetNewItem(e.Parameter as TItem);
    }

    public void SetNewItem(TItem? item)
    {
        Vm = (item switch
        {
            StorageFile t => new FilePageViewModel(t) as T,
            StorageFolder t => new FolderPageViewModel(t) as T,
            _ when typeof(TItem) == typeof(StorageFile) => new FilePageViewModel(null) as T,
            _ when typeof(TItem) == typeof(StorageFolder) => new FolderPageViewModel(null) as T,
            _ => Vm
        })!;
    }

    private void ItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs e)
    {
        if (sender.SelectedItem is not EncodingItem item)
            return;

        Vm.OriginalEncoding = item;
    }

    private void ItemsView2_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs e)
    {
        if (sender.SelectedItem is not EncodingItem item)
            return;

        Vm.DestinationEncoding = item;
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not [EncodingItem item])
            return;

        if (Vm.DetectionDetails?.IndexOf(item) is { } i and not -1)
        {
            OriginalItemsViewOverride.Select(i);
            if (Initialized)
                OriginalItemsViewOverride.StartBringItemIntoView(i, new() { AnimationDesired = true });
        }
        else
            OriginalItemsViewOverride.DeselectAll();
    }

    private void Selector2_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not [EncodingItem item])
            return;

        if (Vm.Encodings?.IndexOf(item) is { } i and not -1)
        {
            DestinationItemsViewOverride.Select(i);
            if (Initialized)
                DestinationItemsViewOverride.StartBringItemIntoView(i, new() { AnimationDesired = true });
        }
        else
            DestinationItemsViewOverride.DeselectAll();
    }

    protected double ItemHeight { get; set; } = -1;

    protected IStorageItemPage EncodeResultItem_OnRequestParent(EncodeResultItem sender, EncodingItem item)
    {
        if (ItemHeight is -1)
        {
            sender.SizeChanged += (_, e) =>
            {
                ItemHeight = e.NewSize.Height;
                PopulateLabelCollection();
            };
        }
        return this;
    }

    private void PopulateLabelCollection()
    {
        AnnotatedScrollBarOverride.Labels.Clear();
        AnnotatedScrollBarOverride.Labels.Add(new("Pinned", 0));

        var set = new HashSet<char>();
        for (var index = 0; index < EncodingHelper.EncodingList.Count; ++index)
        {
            if (EncodingHelper.EncodingList[index].IsPinned)
                continue;
            var capital = EncodingHelper.EncodingList[index].DisplayName[0];
            if (set.Add(capital))
                AnnotatedScrollBarOverride.Labels.Add(new(capital, index * ItemHeight));
        }
    }

    private void AnnotatedScrollBarOverride_DetailLabelRequested(AnnotatedScrollBar sender, AnnotatedScrollBarDetailLabelRequestedEventArgs e)
    {
        var index = Math.Clamp((int)(e.ScrollOffset / ItemHeight), 0, EncodingHelper.EncodingList.Count - 1);
        e.Content = EncodingHelper.EncodingList[index].DisplayName[0];
    }
}
