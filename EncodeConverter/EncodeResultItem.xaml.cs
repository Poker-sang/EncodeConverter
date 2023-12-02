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
    public EncodingInfo Model
    {
        get => _model;
        set
        {
            if (Equals(value, _model))
                return;
            _model = value;
            if (RequestViewModel?.Invoke() is { IsStorageFileNotNull: true } vm)
            {
                _parentViewModel = vm;
                if (ReEncode)
                    vm.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName is nameof(FilePageViewModel.SourceEncoding) && sender.To<FilePageViewModel>().SourceEncoding is not null)
                        {
                            var dstEncoding = Encoding.GetEncoding(_model.CodePage);
                            var name = dstEncoding.GetBytes(_parentViewModel.SourceEncodingName);
                            var content = dstEncoding.GetBytes(_parentViewModel.SourceEncodingContent);
                            PreviewNameTextBlock.Text = NativeHelper.SystemEncoding.GetString(name);
                            PreviewContentTextBlock.Text = NativeHelper.SystemEncoding.GetString(content);
                            PreviewNameTextBlock.Visibility = Visibility.Visible;
                            PreviewContentTextBlock.Visibility = Visibility.Visible;
                        }
                    };
                else
                {
                    var srcEncoding = Encoding.GetEncoding(_model.CodePage);
                    PreviewNameTextBlock.Text = srcEncoding.GetString(_parentViewModel.NameBytes);
                    PreviewContentTextBlock.Text = srcEncoding.GetString(_parentViewModel.ContentBytes);
                    PreviewNameTextBlock.Visibility = Visibility.Visible;
                    PreviewContentTextBlock.Visibility = Visibility.Visible;
                }
            }
            OnPropertyChanged();
        }
    }

    private FilePageViewModel? _parentViewModel;

    public bool ReEncode { get; set; }

    public event Func<FilePageViewModel>? RequestViewModel;

    private EncodingInfo _model = null!;

    public EncodeResultItem()
    {
        InitializeComponent();
    }
}
