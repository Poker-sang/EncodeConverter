using System.Linq;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;

namespace EncodeConverter;

public sealed partial class FilePage : Page
{
    private FilePageViewModel _vm = null!;

    public FilePage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _vm = new(e.Parameter as StorageFile);
    }

    private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
    {
        _vm.SourceEncoding = e.ClickedItem as EncodingInfo;
    }

    private void ListViewBase2_OnItemClick(object sender, ItemClickEventArgs e)
    {
        _vm.DestinationEncoding = e.ClickedItem as EncodingInfo;
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not [EncodingInfo item])
            return;

        if (_vm.DetectionDetails?.IndexOf(item) is { } i and not -1)
        {
            PredictListView.SelectedIndex = i;
            PredictListView.ScrollIntoView(item);
        }
        else
            PredictListView.SelectedIndex = -1;
    }

    private void Selector2_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not [EncodingInfo item])
            return;

        if (_vm.Encodings?.IndexOf(item) is { } i and not -1)
        {
            PreviewListView.SelectedIndex = i;
            PreviewListView.ScrollIntoView(item);
        }
        else
            PreviewListView.SelectedIndex = -1;
    }
}
