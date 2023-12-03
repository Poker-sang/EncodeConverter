using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Input;
using WinUI3Utilities;

namespace EncodeConverter;

[INotifyPropertyChanged]
public sealed partial class FilePage : Page
{
    [ObservableProperty]
    private FilePageViewModel _vm = null!;

    public FilePage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        SetNewFile(e.Parameter as StorageFile);
    }

    public void SetNewFile(StorageFile? file)
    {
        Vm = new(file);
    }

    private void ListViewBase_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not [EncodingItem item])
            return;

        Vm.SourceEncoding = item;
    }

    private void ListViewBase2_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not [EncodingItem item])
            return;

        Vm.DestinationEncoding = item;
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not [EncodingItem item])
            return;

        if (Vm.DetectionDetails?.IndexOf(item) is { } i and not -1)
        {
            PredictListView.SelectedIndex = i;
            PredictListView.ScrollIntoView(item);
        }
        else
            PredictListView.SelectedIndex = -1;
    }

    private void Selector2_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not [EncodingItem item])
            return;

        if (Vm.Encodings?.IndexOf(item) is { } i and not -1)
        {
            PreviewListView.SelectedIndex = i;
            PreviewListView.ScrollIntoView(item);
        }
        else
            PreviewListView.SelectedIndex = -1;
    }

    private FilePage EncodeResultItem_OnRequestParent() => this;

    private async void LoadNewFileOnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (await PickerHelper.PickSingleFileAsync(CurrentContext.Window) is { } file)
            SetNewFile(file);
    }
}
