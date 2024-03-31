using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace EncodeConverter.Pages;

public abstract class TextPageBase : OriginalEncodingsPage<TextPageViewModel>;

public sealed partial class TextPage : TextPageBase
{
    public TextPage()
    {
        Vm = new();
        InitializeComponent();
        SubscribeEvents();
    }

    protected override ItemsView OriginalItemsViewOverride => OriginalItemsView;

    protected override ComboBox OriginalComboBoxOverride => OriginalComboBox;

    private void InputTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        Vm.InputText = InputTextBox.Text;
    }

    private void Detect_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        Vm.Detect();
    }
}
