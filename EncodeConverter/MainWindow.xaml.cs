using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using EncodeConverter.Misc;
using EncodeConverter.Pages;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities;
using Microsoft.UI.Xaml.Navigation;
using WinUI3Utilities.Attributes;

namespace EncodeConverter;

[WindowSizeHelper]
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        MinWidth = 800;
        MinHeight = 400;
        InitializeComponent();
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
            TextPage => NavigationView.MenuItems[2],
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
                e.DragUIOverride.Caption = MainWindowResources.ReadFile;
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
            _ = 0 switch
            {
               0 when tag == MainWindowResources.FileNavigationViewItemContent => ContentFrame.Navigate(typeof(FilePage)),
               0 when tag == MainWindowResources.FolderNavigationViewItemContent => ContentFrame.Navigate(typeof(FolderPage)),
               0 when tag == MainWindowResources.TextNavigationViewItemContent => ContentFrame.Navigate(typeof(TextPage)),
                _ => ThrowHelper.ArgumentOutOfRange<string, bool>(tag)
            };
        }
    }

    private void NewEncodingAutoSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (!ushort.TryParse(sender.Text, out var result))
        {
            NewEncodingTeachingTip.Show(MainWindowResources.Error, TeachingTipSeverity.Error, MainWindowResources.CodePageError, true);
            return;
        }

        if (EncodingHelper.TryGetEncodingItem(result) is { } encodingItem)
        {
            NewEncodingTeachingTip.Show(MainWindowResources.Information, TeachingTipSeverity.Information, string.Format(MainWindowResources.CodePageExisted, encodingItem.DisplayName, encodingItem.CodePage), true);
            return;
        }

        if (EncodingHelper.TryFetchNewEncodingItem(result) is { } newEncodingItem)
        {
            NewEncodingTeachingTip.Show(MainWindowResources.Success, TeachingTipSeverity.Ok, string.Format(MainWindowResources.CodePageAquired, newEncodingItem.DisplayName, newEncodingItem.CodePage), true);
        }
        else
        {
            NewEncodingTeachingTip.Show(MainWindowResources.Error, TeachingTipSeverity.Error, MainWindowResources.CodePageNotFound, true);
        }
    }
}
