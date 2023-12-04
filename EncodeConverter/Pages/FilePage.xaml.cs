using System;
using System.IO;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinUI3Utilities;

namespace EncodeConverter.Pages;

public abstract class StorageFilePage : StorageItemPage<FilePageViewModel, StorageFile, FileInfo>;

public sealed partial class FilePage : StorageFilePage
{
    protected override ItemsView OriginalItemsViewOverride => OriginalItemsView;
    protected override ItemsView DestinationItemsViewOverride => DestinationItemsView;
    protected override ComboBox OriginalComboBoxOverride => OriginalComboBox;
    protected override ComboBox DestinationComboBoxOverride => DestinationComboBox;
    protected override AnnotatedScrollBar AnnotatedScrollBarOverride => AnnotatedScrollBar;

    public FilePage()
    {
        InitializeComponent();
        SubscribeEvents();
    }

    private async void LoadNewFileOnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (await PickerHelper.PickSingleFileAsync(CurrentContext.Window) is { } file)
            SetNewItem(file);
    }

    private async void Transcode_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        await TranscodeHelper.Transcode(Vm.Info!, Vm.OriginalEncoding.CodePage, Vm.DestinationEncoding.CodePage, Vm.KeepOriginal, Vm.TranscodeName, Vm.TranscodeContent);
    }
}
