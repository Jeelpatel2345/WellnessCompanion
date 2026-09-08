using System.Drawing;
using System.Windows.Forms;

namespace WellnessCompanion.Services;

public sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly Icon? _loadedIcon;
    private readonly ToolStripMenuItem _remindersItem;
    private readonly Action<bool> _setRemindersEnabled;
    private bool _remindersEnabled;

    public TrayService(Action showSettings, Action exit, Action<bool> setRemindersEnabled, bool remindersEnabled)
    {
        _setRemindersEnabled = setRemindersEnabled;
        _remindersEnabled = remindersEnabled;
        var menu = new ContextMenuStrip();

        var settingsItem = new ToolStripMenuItem("Open Wellness Companion");
        settingsItem.Click += (_, _) => showSettings();

        _remindersItem = new ToolStripMenuItem();
        _remindersItem.Click += (_, _) => _setRemindersEnabled(!_remindersEnabled);

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => exit();

        menu.Items.Add(settingsItem);
        menu.Items.Add(_remindersItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        try
        {
            using var stream = AssetService.OpenLogoStream();
            if (stream != null)
                _loadedIcon = new Icon(stream);
        }
        catch
        {
            _loadedIcon = null;
        }

        _notifyIcon = new NotifyIcon
        {
            Icon = _loadedIcon ?? SystemIcons.Application,
            Text = "Wellness Companion",
            Visible = true,
            ContextMenuStrip = menu
        };

        UpdateRemindersEnabled(remindersEnabled);

        _notifyIcon.DoubleClick += (_, _) => showSettings();
    }

    public void UpdateRemindersEnabled(bool enabled)
    {
        _remindersEnabled = enabled;
        _remindersItem.Text = enabled ? "Pause reminders" : "Resume reminders";
        _notifyIcon.Text = enabled ? "Wellness Companion — reminders active" : "Wellness Companion — reminders paused";
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _loadedIcon?.Dispose();
    }
}
