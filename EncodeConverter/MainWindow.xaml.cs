using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using EncodeConverter.Pages;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities;
using Microsoft.UI.Xaml.Navigation;

namespace EncodeConverter;

public sealed partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        CurrentContext.Window = this;
        InitializeComponent();
        CurrentContext.TitleBar = TitleBar;
        CurrentContext.TitleTextBlock = TitleTextBlock;
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        var deferral = e.GetDeferral();
        if (await e.DataView.GetStorageItemsAsync() is [var item])
        {
            _ = ContentFrame.Navigate(item switch
            {
                StorageFile => typeof(FilePage),
                StorageFolder => typeof(FolderPage),
                _ => ThrowHelper.ArgumentOutOfRange<IStorageItem, Type>(item)
            }, item);
        }
        deferral.Complete();
    }

    private void ContentFrame_OnNavigated(object sender, NavigationEventArgs e)
    {
        var target = e.Content switch
        {
            FilePage => NavigationView.MenuItems[0],
            FolderPage => NavigationView.MenuItems[1],
            _ => null
        };
        if (NavigationView.SelectedItem != target)
            NavigationView.SelectedItem = target;
    }

    private async void OnDragEnter(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var deferral = e.GetDeferral();
            if (await e.DataView.GetStorageItemsAsync() is not [var item])
            {
                Debug.WriteLine("OnDragEnterNone");
                e.AcceptedOperation = DataPackageOperation.None;
            }
            else
            {
                using var thumbnail = await (item switch
                {
                    StorageFile file => file.GetThumbnailAsync(ThumbnailMode.SingleItem, 80),
                    StorageFolder folder => folder.GetThumbnailAsync(ThumbnailMode.SingleItem, 80),
                    _ => ThrowHelper.ArgumentOutOfRange<IStorageItem, IAsyncOperation<StorageItemThumbnail>>(item)
                });
                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(thumbnail);

                Debug.WriteLine(e.AcceptedOperation);
                e.AcceptedOperation = DataPackageOperation.Move;
                e.DragUIOverride.Caption = "读取文件";
                e.DragUIOverride.SetContentFromBitmapImage(bitmapImage);
                e.DragUIOverride.IsCaptionVisible = true; // Sets if the caption is visible
                e.DragUIOverride.IsContentVisible = true; // Sets if the dragged content is visible
                e.DragUIOverride.IsGlyphVisible = true;
            }
            deferral.Complete();
        }
    }

    private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem is NavigationViewItem { Content: string tag })
        {
            _ = tag switch
            {
                "文件" => ContentFrame.Navigate(typeof(FilePage)),
                "文件夹" => ContentFrame.Navigate(typeof(FolderPage)),
                "文字" => ContentFrame.Navigate(typeof(FilePage)),
                _ => ThrowHelper.ArgumentOutOfRange<string, bool>(tag)
            };
        }
    }

    private void NewEncodingAutoSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (!ushort.TryParse(sender.Text, out var result))
        {
            NewEncodingTeachingTip.Show("错误", TeachingTipSeverity.Error, "请输入正确的代码页", true);
            return;
        }

        if (EncodingHelper.TryGetEncodingItem(result) is { } encodingItem)
        {
            NewEncodingTeachingTip.Show("提示", TeachingTipSeverity.Information, $"该编码已存在：{encodingItem.DisplayName} ({encodingItem.CodePage})", true);
            return;
        }

        if (EncodingHelper.TryFetchNewEncodingItem(result) is { } newEncodingItem)
        {
            NewEncodingTeachingTip.Show("成功", TeachingTipSeverity.Ok, $"已获取：{newEncodingItem.DisplayName} ({newEncodingItem.CodePage})", true);
        }
        else
        {
            NewEncodingTeachingTip.Show("错误", TeachingTipSeverity.Error, "未找到相应编码", true);
        }
    }
}
