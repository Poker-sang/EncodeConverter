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

        Vm.OriginalEncoding = item;
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

    private async void Transcode_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await TranscodeHelper.Transcode(Vm.FileInfo!, Vm.OriginalEncoding.CodePage, Vm.DestinationEncoding.CodePage, Vm.KeepOriginalFile, Vm.TranscodeName, Vm.TranscodeContent);
    }

    private void AutoSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (!ushort.TryParse(sender.Text, out var result))
        {
            InfoBar.Severity = InfoBarSeverity.Error;
            // InfoBar.Title = "错误";
            InfoBar.Message = "请输入正确的代码页";
            InfoBar.IsOpen = true;
            return;
        }

        if (NativeHelper.EncodingList.Find(t => t.CodePage == result) is { })
        {
            InfoBar.Severity = InfoBarSeverity.Informational;
            // InfoBar.Title = "错误";
            InfoBar.Message = "该编码已存在";
            InfoBar.IsOpen = true;
            return;
        }


        if (NativeHelper.TryFetchNewEncodingItem(result) is { } encodingItem)
        {
            InfoBar.Severity = InfoBarSeverity.Success;
            // InfoBar.Title = "成功";
            InfoBar.Message = "已获取" + encodingItem.DisplayName;
            InfoBar.IsOpen = true;
        }
        else
        {
            InfoBar.Severity = InfoBarSeverity.Error;
            // InfoBar.Title = "错误";
            InfoBar.Message = "未找到相应编码";
            InfoBar.IsOpen = true;
        }
    }
}
