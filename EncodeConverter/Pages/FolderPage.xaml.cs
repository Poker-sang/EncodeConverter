using System;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Microsoft.UI.Xaml.Input;
using WinUI3Utilities;
using System.IO;

namespace EncodeConverter.Pages;

public abstract class StorageFolderPage : StorageItemPage<FolderPageViewModel, StorageFolder, DirectoryInfo>;

public sealed partial class FolderPage : StorageFolderPage
{
    protected override ItemsView OriginalItemsViewOverride => OriginalItemsView;
    protected override ItemsView DestinationItemsViewOverride => DestinationItemsView;
    protected override ComboBox OriginalComboBoxOverride => OriginalComboBox;
    protected override ComboBox DestinationComboBoxOverride => DestinationComboBox;
    protected override AnnotatedScrollBar AnnotatedScrollBarOverride => AnnotatedScrollBar;

    public FolderPage()
    {
        InitializeComponent();
        SubscribeEvents();
    }

    private async void LoadNewFolderOnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (await PickerHelper.PickSingleFolderAsync(CurrentContext.Window) is { } folder)
            SetNewItem(folder);
    }

    private async void Transcode_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await TranscodeHelper.TranscodeDirectory(Vm.Info!, Vm.OriginalEncoding.CodePage, Vm.DestinationEncoding.CodePage,
                Vm.TranscodeName, Vm.TranscodeContent, _ => true);
    }
}
