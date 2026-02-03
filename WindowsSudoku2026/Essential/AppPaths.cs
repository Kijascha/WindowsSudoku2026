using System.Configuration;
using System.IO;

namespace WindowsSudoku2026.Essential;

public class AppPaths : IAppPaths
{
    public string AppSettingsFile { get; }
    public string UserSettingsFile { get; }

    public AppPaths()
    {
        string baseDir = AppContext.BaseDirectory;
        var appSettingsFile = ConfigurationManager.AppSettings["AppSettingsFile"] ?? throw new ConfigurationErrorsException(
                "Missing appSetting: AppSettingsFile");
        var userSettingsFile = ConfigurationManager.AppSettings["UserSettingsFile"] ?? throw new ConfigurationErrorsException(
                "Missing userSetting: UserSettingsFile");

        AppSettingsFile = Path.Combine(baseDir, appSettingsFile);
        UserSettingsFile = Path.Combine(baseDir, userSettingsFile);
    }
}
