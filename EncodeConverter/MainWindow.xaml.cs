using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities;

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
                StorageFile file => typeof(FilePage),
                StorageFolder folder => typeof(FolderPage),
                _ => ThrowHelper.ArgumentOutOfRange<IStorageItem, Type>(item)
            }, item);
        }
        deferral.Complete();
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
            switch (tag)
            {
                case "文件":
                    ContentFrame.Navigate(typeof(FilePage));
                    break;
                case "文件夹":
                    ContentFrame.Navigate(typeof(FolderPage));
                    break;
                case "文字":
                    // ContentFrame.Navigate(typeof(AboutPage));
                    break;
                default:
                    ThrowHelper.ArgumentOutOfRange(tag);
                    break;
            }
        }
    }
}
