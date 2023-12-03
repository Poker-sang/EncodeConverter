using Windows.Storage;
using WinUI3Utilities.Attributes;

namespace EncodeConverter;

[AppContext<AppSettings>]
public static partial class AppContext
{
    public static string AppLocalFolder { get; private set; } = null!;

    public static void Initialize()
    {
        AppLocalFolder = ApplicationData.Current.LocalFolder.Path;
        InitializeConfigurationContainer();
        AppSetting = LoadConfiguration() is not { } appConfigurations
#if FIRST_TIME
        || true
#endif
            ? new() : appConfigurations;
    }

    public static AppSettings AppSetting { get; private set; } = null!;
}
