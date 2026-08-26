using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WellnessCompanion.Services;

namespace WellnessCompanion.Windows;

public partial class ReminderOverlay : Window
{
    private readonly DispatcherTimer _countdownTimer;
    private readonly bool _isMandatory;
    private int _remainingSeconds;

    public ReminderOverlay(ReminderType type, int durationSeconds, bool isMandatory)
    {
        InitializeComponent();

        _isMandatory = isMandatory;
        _remainingSeconds = Math.Max(1, durationSeconds);

        if (type == ReminderType.Water)
        {
            EmojiText.Text = "💧";
            TitleText.Text = "TIME TO DRINK WATER";
            SubtitleText.Text = "Take a short hydration break. This reminder will stay on screen until the timer ends.";
            FoodButtons.Visibility = Visibility.Collapsed;
        }
        else
        {
            WaterVisual.Visibility = Visibility.Collapsed;
            EmojiText.Text = "Food break";
            EmojiText.Visibility = Visibility.Visible;
            EmojiText.Text = "🍱";
            TitleText.Text = "TIME FOR A FOOD BREAK";
            SubtitleText.Text = "Step away from the screen and take a proper break.";
            FoodButtons.Visibility = Visibility.Visible;
        }

        UpdateCountdown();

        _countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _countdownTimer.Tick += CountdownTimer_Tick;
        _countdownTimer.Start();

        Loaded += (_, _) =>
        {
            Activate();
            Topmost = true;

            if (type == ReminderType.Water)
            {
                ((Storyboard)FindResource("WaterPulse")).Begin();
            }
        };
    }

    private void CountdownTimer_Tick(object? sender, EventArgs e)
    {
        _remainingSeconds--;

        if (_remainingSeconds <= 0)
        {
            _countdownTimer.Stop();
            Close();
            return;
        }

        UpdateCountdown();
    }

    private void UpdateCountdown()
    {
        var time = TimeSpan.FromSeconds(_remainingSeconds);
        CountdownText.Text = time.TotalHours >= 1
            ? time.ToString(@"hh\:mm\:ss")
            : time.ToString(@"mm\:ss");
    }

    private void SetDuration(int minutes)
    {
        _remainingSeconds = minutes * 60;
        UpdateCountdown();
    }

    private void Shorten10_Click(object sender, RoutedEventArgs e) => SetDuration(10);
    private void Shorten15_Click(object sender, RoutedEventArgs e) => SetDuration(15);
    private void Shorten20_Click(object sender, RoutedEventArgs e) => SetDuration(20);

    private void Finish_Click(object sender, RoutedEventArgs e)
    {
        _countdownTimer.Stop();
        Close();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (_isMandatory && _remainingSeconds > 0)
        {
            e.Cancel = true;
            return;
        }

        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        _countdownTimer.Stop();
        base.OnClosed(e);
    }
}
