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
                    {
                        viewModel.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName is nameof(FilePageViewModel.SourceEncoding))
                                SetWhenSourceEncodingChanged(sender.To<FilePageViewModel>());
                        };
                        SetWhenSourceEncodingChanged(viewModel);
                    }
                    else
                    {
                        var srcEncoding = Encoding.GetEncoding(_model.CodePage);
                        SetTextBlocks(srcEncoding, viewModel, viewModel.NameBytes, viewModel.ContentBytes);
                    }
                }
                return;

                void SetWhenSourceEncodingChanged(FilePageViewModel vm)
                {
                    var dstEncoding = Encoding.GetEncoding(_model.CodePage);
                    var name = dstEncoding.GetBytes(vm.SourceEncodingName);
                    var content = dstEncoding.GetBytes(vm.SourceEncodingContent);
                    SetTextBlocks(NativeHelper.SystemEncoding, vm, name, content);
                }

                void SetTextBlocks(Encoding encoding, FilePageViewModel vm, byte[] name, byte[] content)
                {
                    PreviewNameTextBlock.Text = encoding.GetString(name);
                    PreviewContentTextBlock.Text = encoding.GetString(content);
                    PreviewNameTextBlock.Visibility = vm.ParseName ? Visibility.Visible : Visibility.Collapsed;
                    PreviewContentTextBlock.Visibility = vm.ParseContent ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
    }

    public bool ReEncode { get; set; }

    public event Func<FilePage>? RequestParent;

    private EncodingItem _model = null!;

    public EncodeResultItem()
    {
        InitializeComponent();
    }
}
