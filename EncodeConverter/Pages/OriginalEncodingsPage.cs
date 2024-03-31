#region Copyright

// GPL v3 License
// 
// EncodeConverter/EncodeConverter
// Copyright (c) 2024 EncodeConverter/OriginalEncodingsPage.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using CommunityToolkit.Mvvm.ComponentModel;
using EncodeConverter.Misc;
using Microsoft.UI.Xaml.Controls;

namespace EncodeConverter.Pages;

[INotifyPropertyChanged]
public abstract partial class OriginalEncodingsPage<T> : Page where T : AbstractViewModel
{
    [ObservableProperty]
    private T _vm = null!;

    protected OriginalEncodingsPage()
    {
        Loaded += (_, _) => Initialized = true;
        Unloaded += (_, _) => Initialized = false;
    }

    public bool Initialized { get; private set; }

    protected abstract ItemsView OriginalItemsViewOverride { get; }

    protected abstract ComboBox OriginalComboBoxOverride { get; }

    protected virtual void SubscribeEvents()
    {
        OriginalItemsViewOverride.SelectionChanged += ItemsView_SelectionChanged;
        OriginalComboBoxOverride.SelectionChanged += Selector_OnSelectionChanged;
    }

    private void ItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs e)
    {
        if (sender.SelectedItem is not EncodingItem item)
            return;

        Vm.OriginalEncoding = item;
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is not [EncodingItem item])
            return;

        if (Vm.DetectionDetails?.IndexOf(item) is { } i and not -1)
        {
            OriginalItemsViewOverride.Select(i);
            if (Initialized)
                OriginalItemsViewOverride.StartBringItemIntoView(i, new() { AnimationDesired = true });
        }
        else
            OriginalItemsViewOverride.DeselectAll();
    }
}
