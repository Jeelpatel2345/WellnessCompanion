using System.Windows;
using WellnessCompanion.Models;
using WellnessCompanion.Services;

namespace WellnessCompanion;

public partial class MainWindow : Window
{
    private readonly AppSettings _settings;
    private readonly Action<bool> _setRemindersEnabled;
    private bool _allowClose;

    public MainWindow(AppSettings settings, Action<bool> setRemindersEnabled)
    {
        InitializeComponent();
        _settings = settings;
        _setRemindersEnabled = setRemindersEnabled;
        LoadSettings();
    }

    private void LoadSettings()
    {
        WaterIntervalBox.Text = _settings.WaterIntervalMinutes.ToString();
        WaterDurationBox.Text = _settings.WaterDurationSeconds.ToString();
        FoodIntervalBox.Text = _settings.FoodIntervalMinutes.ToString();
        FoodDurationBox.Text = _settings.FoodDurationMinutes.ToString();
        IdleThresholdBox.Text = _settings.IdleThresholdSeconds.ToString();
        SoundBox.IsChecked = _settings.SoundEnabled;
        StartupBox.IsChecked = _settings.StartWithWindows;
        RemindersEnabledBox.IsChecked = _settings.RemindersEnabled;
        UpdateReminderStatus(_settings.RemindersEnabled);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(WaterIntervalBox.Text, out var waterInterval) ||
            !int.TryParse(WaterDurationBox.Text, out var waterDuration) ||
            !int.TryParse(FoodIntervalBox.Text, out var foodInterval) ||
            !int.TryParse(FoodDurationBox.Text, out var foodDuration) ||
            !int.TryParse(IdleThresholdBox.Text, out var idleThreshold))
        {
            System.Windows.MessageBox.Show(
                "Please enter valid whole numbers.",
                "Invalid settings",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (waterInterval < 1 || waterDuration < 1 ||
            foodInterval < 1 || foodDuration < 1 || idleThreshold < 5)
        {
            System.Windows.MessageBox.Show(
                "Use positive values. Idle threshold must be at least 5 seconds.",
                "Invalid settings",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        _settings.WaterIntervalMinutes = waterInterval;
        _settings.WaterDurationSeconds = waterDuration;
        _settings.FoodIntervalMinutes = foodInterval;
        _settings.FoodDurationMinutes = foodDuration;
        _settings.IdleThresholdSeconds = idleThreshold;
        _settings.SoundEnabled = SoundBox.IsChecked == true;
        _settings.StartWithWindows = StartupBox.IsChecked == true;
        _settings.RemindersEnabled = RemindersEnabledBox.IsChecked == true;

        SettingsService.Save(_settings);
        StartupService.SetEnabled(_settings.StartWithWindows);
        _setRemindersEnabled(_settings.RemindersEnabled);

        System.Windows.MessageBox.Show(
            "Settings saved. New reminder intervals are active immediately.",
            "Saved",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    public void ShowFromTray()
    {
        Show();
        if (WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;
        Activate();
        Topmost = true;
        Topmost = false;
    }

    public void SetRemindersEnabledFromTray(bool enabled)
    {
        _settings.RemindersEnabled = enabled;
        RemindersEnabledBox.IsChecked = enabled;
        UpdateReminderStatus(enabled);
    }

    private void UpdateReminderStatus(bool enabled)
    {
        ReminderStatusText.Text = enabled ? "Reminders active" : "Reminders paused";
    }

    public void ExitApplication()
    {
        _allowClose = true;
        Close();
        System.Windows.Application.Current.Shutdown();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => Hide();

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!_allowClose)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }
}
