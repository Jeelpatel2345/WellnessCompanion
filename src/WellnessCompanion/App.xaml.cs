using System.Threading;
using System.Windows;
using WellnessCompanion.Models;
using WellnessCompanion.Services;

namespace WellnessCompanion;

public partial class App : System.Windows.Application
{
    private Mutex? _singleInstanceMutex;
    private ReminderService? _reminderService;
    private TrayService? _trayService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        const string mutexName = "WellnessCompanion.SingleInstance";
        _singleInstanceMutex = new Mutex(true, mutexName, out bool createdNew);

        if (!createdNew)
        {
            Shutdown();
            return;
        }

        var settings = SettingsService.Load();
        StartupService.SetEnabled(settings.StartWithWindows);

        MainWindow? mainWindow = null;

        void SetRemindersEnabled(bool enabled)
        {
            settings.RemindersEnabled = enabled;
            SettingsService.Save(settings);
            _reminderService?.SetEnabled(enabled);
            _trayService?.UpdateRemindersEnabled(enabled);
            mainWindow?.SetRemindersEnabledFromTray(enabled);
        }

        mainWindow = new MainWindow(settings, SetRemindersEnabled);
        MainWindow = mainWindow;

        _trayService = new TrayService(
            showSettings: () => Dispatcher.Invoke(mainWindow.ShowFromTray),
            exit: () => Dispatcher.Invoke(mainWindow.ExitApplication),
            setRemindersEnabled: enabled => Dispatcher.Invoke(() => SetRemindersEnabled(enabled)),
            remindersEnabled: settings.RemindersEnabled);

        _reminderService = new ReminderService(settings);
        _reminderService.Start();

        mainWindow.Show();

        if (e.Args.Any(arg => string.Equals(arg, "--startup", StringComparison.OrdinalIgnoreCase)))
            mainWindow.Hide();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _reminderService?.Stop();
        _trayService?.Dispose();
        _singleInstanceMutex?.Dispose();

        base.OnExit(e);
    }
}
