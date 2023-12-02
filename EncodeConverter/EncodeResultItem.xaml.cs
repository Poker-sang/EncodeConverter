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
            if (RequestViewModel?.Invoke() is { IsStorageFileNotNull: true } viewModel)
            {
                _parentViewModel = viewModel;
                viewModel.PropertyChanged += (sender, args) =>
                {
                    var vm = sender.To<FilePageViewModel>();
                    switch (args.PropertyName)
                    {
                        case nameof(FilePageViewModel.ParseName):
                        {
                            PreviewNameTextBlock.Visibility = vm.ParseName ? Visibility.Visible : Visibility.Collapsed;
                            break;
                        }
                        case nameof(FilePageViewModel.ParseContent):
                        {
                            PreviewContentTextBlock.Visibility = vm.ParseContent ? Visibility.Visible : Visibility.Collapsed;
                            break;
                        }
                    }
                };
                if (ReEncode)
                    viewModel.PropertyChanged += (sender, args) =>
                    {
                        var vm = sender.To<FilePageViewModel>();
                        if (args.PropertyName == nameof(FilePageViewModel.SourceEncoding) &&
                            sender.To<FilePageViewModel>().SourceEncoding is not null)
                        {
                            var dstEncoding = Encoding.GetEncoding(_model.CodePage);
                            var name = dstEncoding.GetBytes(_parentViewModel.SourceEncodingName);
                            var content = dstEncoding.GetBytes(_parentViewModel.SourceEncodingContent);
                            PreviewNameTextBlock.Text = NativeHelper.SystemEncoding.GetString(name);
                            PreviewContentTextBlock.Text = NativeHelper.SystemEncoding.GetString(content);
                            PreviewNameTextBlock.Visibility =
                                vm.ParseName ? Visibility.Visible : Visibility.Collapsed;
                            PreviewContentTextBlock.Visibility =
                                vm.ParseContent ? Visibility.Visible : Visibility.Collapsed;
                        }
                    };
                else
                {
                    var srcEncoding = Encoding.GetEncoding(_model.CodePage);
                    PreviewNameTextBlock.Text = srcEncoding.GetString(_parentViewModel.NameBytes);
                    PreviewContentTextBlock.Text = srcEncoding.GetString(_parentViewModel.ContentBytes);
                    PreviewNameTextBlock.Visibility = viewModel.ParseName ? Visibility.Visible : Visibility.Collapsed;
                    PreviewContentTextBlock.Visibility =
                        viewModel.ParseContent ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            OnPropertyChanged();
        }
    }

    private FilePageViewModel? _parentViewModel;

    public bool ReEncode { get; set; }

    public event Func<FilePageViewModel>? RequestViewModel;

    private EncodingItem _model = null!;

    public EncodeResultItem()
    {
        InitializeComponent();
    }
}
