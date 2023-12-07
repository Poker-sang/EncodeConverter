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
        CurrentContext.Title = nameof(EncodeConverter);
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var mainWindow = new MainWindow();
        mainWindow.Initialize(new()
        {
            Size = WindowHelper.EstimatedWindowSize(),
            TitleBarType = TitleBarType.Window
        }, nameof(EncodeConverter), CurrentContext.TitleBar);
        mainWindow.Activate();
    }
}
