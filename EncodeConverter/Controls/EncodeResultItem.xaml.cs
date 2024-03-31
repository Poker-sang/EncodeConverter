using System;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using EncodeConverter.Misc;
using EncodeConverter.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities;

namespace EncodeConverter.Controls;

[INotifyPropertyChanged]
public sealed partial class EncodeResultItem : UserControl
{
    public EncodingItem Model
    {
        get => _model;
        set
        {
            if (Equals(value, _model))
                return;
            _model = value;
            if (RequestParent?.Invoke(this, _model) is { } page)
            {
                page.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName is nameof(IStorageItemPage.Vm))
                        SetViewModel(sender.To<IStorageItemPage>().Vm);
                };
                SetViewModel(page.Vm);
            }
            OnPropertyChanged();
            return;

            void SetViewModel(IStorageItemPageViewModel viewModel)
            {
                if (viewModel is { IsStorageItemNotNull: true })
                {
                    viewModel.PropertyChanged += (sender, args) =>
                    {
                        var vm = sender.To<IStorageItemPageViewModel>();
                        if (args.PropertyName == nameof(IStorageItemPageViewModel.TranscodeContent))
                            PreviewContentTextBlock.Visibility = vm.TranscodeContent ? Visibility.Visible : Visibility.Collapsed;
                    };
                    if (Transcode)
                    {
                        viewModel.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName is nameof(IStorageItemPageViewModel.OriginalEncoding))
                                SetWhenOriginalEncodingChanged(sender.To<IStorageItemPageViewModel>());
                        };
                        SetWhenOriginalEncodingChanged(viewModel);
                    }
                    else
                    {
                        viewModel.PropertyChanged += (sender, args) =>
                        {
                            var vm = sender.To<IStorageItemPageViewModel>();
                            if (args.PropertyName is nameof(IStorageItemPageViewModel.TranscodeName))
                                PreviewNameTextBlock.Visibility = vm.TranscodeName ? Visibility.Visible : Visibility.Collapsed;
                        };
                        var srcEncoding = Encoding.GetEncoding(_model.CodePage);
                        SetTextBlocks(srcEncoding, viewModel, viewModel.NameBytes, viewModel.ContentBytes);
                    }
                }
                return;

                void SetWhenOriginalEncodingChanged(IStorageItemPageViewModel vm)
                {
                    var dstEncoding = Encoding.GetEncoding(_model.CodePage);
                    PreviewContentTextBlock.Text = TranscodeHelper.TranscodeStringToNative(vm.OriginalEncodingContent, dstEncoding);
                    PreviewContentTextBlock.Visibility = vm.TranscodeContent ? Visibility.Visible : Visibility.Collapsed;
                }

                void SetTextBlocks(Encoding encoding, IStorageItemPageViewModel vm, byte[] name, byte[] content)
                {
                    PreviewNameTextBlock.Text = encoding.GetString(name);
                    PreviewContentTextBlock.Text = encoding.GetString(content);
                    PreviewNameTextBlock.Visibility = vm.TranscodeName ? Visibility.Visible : Visibility.Collapsed;
                    PreviewContentTextBlock.Visibility = vm.TranscodeContent ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
    }

    public bool Transcode { get; set; }

    public event Func<EncodeResultItem, EncodingItem, IStorageItemPage>? RequestParent;

    private EncodingItem _model = null!;

    public EncodeResultItem() => InitializeComponent();
}
