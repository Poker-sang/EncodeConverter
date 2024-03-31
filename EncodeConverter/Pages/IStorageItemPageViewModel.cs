#region Copyright

// GPL v3 License
// 
// EncodeConverter/EncodeConverter
// Copyright (c) 2024 EncodeConverter/IStorageItemPageViewModel.cs
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using EncodeConverter.Misc;

namespace EncodeConverter.Pages;

public interface IStorageItemPageViewModel : INotifyPropertyChanged
{
    [MemberNotNullWhen(true, nameof(Path), nameof(NameBytes), nameof(Name), nameof(ContentBytes), nameof(Content), nameof(DetectionDetails))]
    bool IsStorageItemNotNull { get; }

    string? Path { get; }

    byte[]? NameBytes { get; }

    string? Name { get; }

    byte[]? ContentBytes { get; }

    string? Content { get; }

    string OriginalEncodingName { get; set; }

    string OriginalEncodingContent { get; set; }

    EncodingItem OriginalEncoding { get; set; }

    EncodingItem DestinationEncoding { get; set; }

    bool TranscodeName { get; set; }

    bool TranscodeContent { get; set; }

    bool DestinationEncodingUseSystem { get; set; }

    List<EncodingItem>? DetectionDetails { get; }
}
