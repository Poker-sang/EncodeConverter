using System;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Microsoft.UI.Xaml.Input;
using WinUI3Utilities;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EncodeConverter.Misc;

namespace EncodeConverter.Pages;

public abstract class FolderPageBase : StorageItemPage<FolderPageViewModel, StorageFolder, DirectoryInfo>;

public sealed partial class FolderPage : FolderPageBase
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
        if (await App.MainWindow.PickSingleFolderAsync() is { } folder)
            SetNewItem(folder);
    }

    private async void Transcode_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        Regex regex = null!;
        if (Vm.FileFilter is FileFilterType.UseRegex)
        {
            try
            {
                regex = new(Vm.FilterRegex);
            }
            catch (Exception exception)
            {
                RegexTeachingTip.Show(FolderPageResources.RegexError, TeachingTipSeverity.Error, exception.Message);
                return;
            }
        }

        var func = Vm.FileFilter switch
        {
            FileFilterType.TxtOnly => (_, ext) => ".txt".Equals(ext, StringComparison.OrdinalIgnoreCase),
            FileFilterType.SpecifyExtensions => (_, ext) =>
            {
                var comparison = Vm.FilterExtensionsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                return Vm.FilterExtensions.Split(';', StringSplitOptions.RemoveEmptyEntries).Any(extension => ext.Equals(extension, comparison));
            }
            ,
            FileFilterType.UseRegex => (name, _) => regex.IsMatch(name),
            _ => ThrowHelper.ArgumentOutOfRange<FileFilterType, Func<string, string, bool>>(Vm.FileFilter)
        };
        await TranscodeHelper.TranscodeDirectory(Vm.Info!, Vm.OriginalEncoding.CodePage, Vm.DestinationEncoding.CodePage,
                Vm.TranscodeName, Vm.TranscodeContent, func);
    }
}
