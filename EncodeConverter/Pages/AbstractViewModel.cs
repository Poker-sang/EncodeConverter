#region Copyright

// GPL v3 License
// 
// EncodeConverter/EncodeConverter
// Copyright (c) 2024 EncodeConverter/AbstractViewModel.cs
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EncodeConverter.Misc;

namespace EncodeConverter.Pages;

public abstract class AbstractViewModel : ObservableObject
{
    public List<EncodingItem>? DetectionDetails { get; protected set; }

    public EncodingItem OriginalEncoding
    {
        get => EncodingHelper.GetEncodingItemOrFetch(SourceOriginalEncodingCodePage);
        set
        {
            if (value.CodePage == SourceOriginalEncodingCodePage)
                return;
            SourceOriginalEncodingCodePage = value.CodePage;
            AppContext.SaveConfiguration(AppSetting);
            OnOriginalEncodingChanged();
            OnPropertyChanged();
        }
    }

    protected abstract void OnOriginalEncodingChanged();

    public EncodingItem DestinationEncoding
    {
        get => EncodingHelper.GetEncodingItemOrFetch(SourceDestinationEncodingCodePage);
        set
        {
            if (value.CodePage == SourceDestinationEncodingCodePage)
                return;
            SourceDestinationEncodingCodePage = value.CodePage;
            AppContext.SaveConfiguration(AppSetting);
            OnPropertyChanged();
        }
    }

    public bool DestinationEncodingUseSystem
    {
        get => SourceDestinationEncodingUseSystem;
        set
        {
            if (value == SourceDestinationEncodingUseSystem)
                return;
            SourceDestinationEncodingUseSystem = value;
            AppContext.SaveConfiguration(AppSetting);
            DestinationEncoding = EncodingHelper.SystemEncodingInfo;
            OnPropertyChanged();
        }
    }

    protected virtual int SourceOriginalEncodingCodePage
    {
        get => AppSetting.FileOriginalEncodingCodePage;
        set => AppSetting.FileOriginalEncodingCodePage = value;
    }

    protected virtual int SourceDestinationEncodingCodePage
    {
        get => AppSetting.FileDestinationEncodingCodePage;
        set => AppSetting.FileDestinationEncodingCodePage = value;
    }

    protected bool SourceDestinationEncodingUseSystem
    {
        get => AppSetting.TextDestinationEncodingUseSystem;
        set => AppSetting.TextDestinationEncodingUseSystem = value;
    }

#pragma warning disable CA1822 // 将成员标记为 static
    public ObservableCollection<EncodingItem> Encodings => EncodingHelper.EncodingCollection;

    public AppSettings AppSetting => AppContext.AppSettings;
#pragma warning restore CA1822 // 将成员标记为 static
}
