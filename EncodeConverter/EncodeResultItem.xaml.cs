using System;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using WinUI3Utilities;

namespace EncodeConverter;

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
            if (RequestParent?.Invoke() is { } page)
            {
                page.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName is nameof(FilePage.Vm))
                        SetViewModel(sender.To<FilePage>().Vm);
                };
                SetViewModel(page.Vm);
            }
            OnPropertyChanged();
            return;

            void SetViewModel(FilePageViewModel viewModel)
            {
                if (viewModel is { IsStorageFileNotNull: true })
                {
                    viewModel.PropertyChanged += (sender, args) =>
                    {
                        var vm = sender.To<FilePageViewModel>();
                        if (args.PropertyName == nameof(FilePageViewModel.TranscodeContent))
                            PreviewContentTextBlock.Visibility = vm.TranscodeContent ? Visibility.Visible : Visibility.Collapsed;
                    };
                    if (Transcode)
                    {
                        viewModel.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName is nameof(FilePageViewModel.OriginalEncoding))
                                SetWhenOriginalEncodingChanged(sender.To<FilePageViewModel>());
                        };
                        SetWhenOriginalEncodingChanged(viewModel);
                    }
                    else
                    {
                        viewModel.PropertyChanged += (sender, args) =>
                        {
                            var vm = sender.To<FilePageViewModel>();
                            if (args.PropertyName is nameof(FilePageViewModel.TranscodeName))
                                PreviewNameTextBlock.Visibility = vm.TranscodeName ? Visibility.Visible : Visibility.Collapsed;
                        };
                        var srcEncoding = Encoding.GetEncoding(_model.CodePage);
                        SetTextBlocks(srcEncoding, viewModel, viewModel.NameBytes, viewModel.ContentBytes);
                    }
                }
                return;

                void SetWhenOriginalEncodingChanged(FilePageViewModel vm)
                {
                    var dstEncoding = Encoding.GetEncoding(_model.CodePage);
                    var content = dstEncoding.GetBytes(vm.OriginalEncodingContent);
                    PreviewContentTextBlock.Text = NativeHelper.SystemEncoding.GetString(content);
                    PreviewContentTextBlock.Visibility = vm.TranscodeContent ? Visibility.Visible : Visibility.Collapsed;
                }

                void SetTextBlocks(Encoding encoding, FilePageViewModel vm, byte[] name, byte[] content)
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

    public event Func<FilePage>? RequestParent;

    private EncodingItem _model = null!;

    public EncodeResultItem()
    {
        InitializeComponent();
    }
}
