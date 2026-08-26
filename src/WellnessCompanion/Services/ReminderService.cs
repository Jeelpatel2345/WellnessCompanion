using System.Media;
using System.Windows.Threading;
using WellnessCompanion.Models;
using WellnessCompanion.Windows;

namespace WellnessCompanion.Services;

public sealed class ReminderService
{
    private readonly AppSettings _settings;
    private readonly DispatcherTimer _timer;
    private TimeSpan _sinceWater = TimeSpan.Zero;
    private TimeSpan _sinceFood = TimeSpan.Zero;
    private bool _reminderOpen;

    public ReminderService(AppSettings settings)
    {
        _settings = settings;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _timer.Tick += OnTick;
    }

    public void Start() => _timer.Start();

    public void Stop() => _timer.Stop();

    public void ResetCounters()
    {
        _sinceWater = TimeSpan.Zero;
        _sinceFood = TimeSpan.Zero;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_reminderOpen)
            return;

        var idle = IdleService.GetIdleTime();
        if (idle.TotalSeconds >= _settings.IdleThresholdSeconds)
            return;

        _sinceWater += TimeSpan.FromSeconds(1);
        _sinceFood += TimeSpan.FromSeconds(1);

        if (_sinceWater.TotalMinutes >= _settings.WaterIntervalMinutes)
        {
            _sinceWater = TimeSpan.Zero;
            ShowWater();
            return;
        }

        if (_sinceFood.TotalMinutes >= _settings.FoodIntervalMinutes)
        {
            _sinceFood = TimeSpan.Zero;
            ShowFood();
        }
    }

    private void PlayReminderSound()
    {
        if (_settings.SoundEnabled)
            SystemSounds.Asterisk.Play();
    }

    private void ShowWater()
    {
        _reminderOpen = true;
        PlayReminderSound();

        var overlay = new ReminderOverlay(
            ReminderType.Water,
            _settings.WaterDurationSeconds,
            isMandatory: true);

        overlay.Owner = System.Windows.Application.Current.MainWindow;
        overlay.Closed += async (_, _) =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_settings.WaterVerificationDelaySeconds));
                await VerifyWaterAsync();
            }
            finally
            {
                _reminderOpen = false;
            }
        };

        overlay.Show();
    }

    private async Task VerifyWaterAsync()
    {
        for (var attempt = 1; attempt <= _settings.MaxWaterVerificationAttempts; attempt++)
        {
            var verification = new WaterVerification(attempt, _settings.MaxWaterVerificationAttempts);
            verification.Owner = System.Windows.Application.Current.MainWindow;
            verification.ShowDialog();

            if (verification.Verified)
            {
                _sinceWater = TimeSpan.Zero;
                return;
            }

            if (attempt < _settings.MaxWaterVerificationAttempts)
                await Task.Delay(TimeSpan.FromSeconds(_settings.WaterRetryDelaySeconds));
        }

        _sinceWater = TimeSpan.Zero;
    }

    private void ShowFood()
    {
        _reminderOpen = true;
        PlayReminderSound();

        var overlay = new ReminderOverlay(
            ReminderType.Food,
            _settings.FoodDurationMinutes * 60,
            isMandatory: false);

        overlay.Owner = System.Windows.Application.Current.MainWindow;
        overlay.Closed += (_, _) => _reminderOpen = false;
        overlay.Show();
    }
}

public enum ReminderType
{
    Water,
    Food
}
