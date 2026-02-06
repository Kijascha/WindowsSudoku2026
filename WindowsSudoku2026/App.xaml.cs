using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Windows;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Common.Settings;
using WindowsSudoku2026.Core.Interfaces;
using WindowsSudoku2026.Core.Services;
using WindowsSudoku2026.Essential;
using WindowsSudoku2026.Infrastructure.Services;
using WindowsSudoku2026.Services;
using WindowsSudoku2026.ViewModels;
using WindowsSudoku2026.Views;

namespace WindowsSudoku2026
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }
        public App()
        {
            InitializeComponent();

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    var appConfig = System.Configuration.ConfigurationManager.AppSettings;

                    string jsonAppSettings = appConfig["AppSettingsFile"] ?? throw new System.Configuration.ConfigurationErrorsException("Missing appSetting: AppSettingsFile");
                    string jsonUserSettings = appConfig["UserSettingsFile"] ?? throw new System.Configuration.ConfigurationErrorsException("Missing userSetting: UserSettingsFile");

                    configuration.AddJsonFile(jsonAppSettings, optional: false, reloadOnChange: true);
                    configuration.AddJsonFile(jsonUserSettings, optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Zugriff auf Configuration HIER möglich
                    IConfiguration configuration = context.Configuration;
                    // Im Host-Builder oder Service-Setup
                    services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
                    services.Configure<UserSettings>(configuration.GetSection("UserSettings"));

                    services.AddSingleton<IOptionsMonitor<UserSettings>, OptionsMonitor<UserSettings>>();
                    services.AddSingleton<IAppPaths, AppPaths>();
                    //services.AddSingleton<ISQLiteService, SQLiteService>();
                    //services.AddSingleton<IDtoSqlService, DtoSqlService>();
                    services.AddSingleton<ITimerService, TimerService>();
                    //services.AddSingleton<ISettingsService, SettingsService>();
                    //services.AddSingleton<IJsonService, JsonService>();
                    //services.AddSingleton<IDtoJsonService, DtoJsonService>();
                    //services.AddSingleton<IColorPaletteService, ColorPaletteService>();
                    //services.AddSingleton<IGameService, GameService>();
                    services.AddSingleton<INavigationService, NavigationService>();

                    services.AddPuzzleFactory<Puzzle>();

                    services.AddInfrastucture();
                    services.AddCoreServices();

                    services.AddAbstractFactory<ColorPickerViewModel>();
                    services.AddAbstractFactory<SettingsViewModel>();
                    services.AddAbstractFactory<ColorPaletteSelectionViewModel>();
                    services.AddAbstractFactory<StartupViewModel>();
                    services.AddAbstractFactory<MenuViewModel>();
                    services.AddAbstractFactory<PlayMenuViewModel>();
                    services.AddAbstractFactory<PuzzleSelectionViewModel>();
                    services.AddAbstractFactory<PlayViewModel>();
                    services.AddAbstractFactory<CreateViewModel>();
                    services.AddAbstractFactory<SavePuzzleDialogViewModel>();
                    services.AddAbstractFactory<NotificationViewModel>();

                    services.AddSingleton(p => new SettingsView()
                    {
                        DataContext = p.GetRequiredService<SettingsViewModel>()
                    });
                    services.AddSingleton(p => new StartupView()
                    {
                        DataContext = p.GetRequiredService<StartupViewModel>()
                    });
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var startupForm = AppHost!.Services.GetRequiredService<StartupView>();
            startupForm.Show();


            base.OnStartup(e);
        }
        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();

            base.OnExit(e);
        }
    }
}
