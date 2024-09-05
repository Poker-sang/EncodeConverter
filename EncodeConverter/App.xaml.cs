using EncodeConverter.Misc;
using Microsoft.UI.Xaml;
using WinUI3Utilities;

namespace EncodeConverter;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        AppContext.Initialize();
        EncodingHelper.Initialize();
    }

    public static MainWindow MainWindow { get; private set; } = null!;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new();
        MainWindow.Initialize(new()
        {
            Title = AppContext.Title,
            Size = WindowHelper.EstimatedWindowSize(),
        });
        MainWindow.Activate();
    }
}
