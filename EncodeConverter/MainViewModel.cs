using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EncodeConverter;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Path), nameof(Name), nameof(Extension))]
    private FileInfo? _file;

    public string? Path => File?.DirectoryName;

    public string? Name => File?.Name;

    public string? Extension => File?.Extension;

    [ObservableProperty]
    private string _content = "";
}
