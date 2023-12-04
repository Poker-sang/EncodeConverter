#define FIRST_TIME
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
        AppSettings = LoadConfiguration() is not { } appConfigurations
#if FIRST_TIME
        || true
#endif
            ? new() : appConfigurations;
    }

    public static AppSettings AppSettings { get; private set; } = null!;
}
