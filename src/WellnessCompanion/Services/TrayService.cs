using System.Drawing;
using System.Windows.Forms;

namespace WellnessCompanion.Services;

public sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly Icon? _loadedIcon;

    public TrayService(Action showSettings, Action exit)
    {
        var menu = new ContextMenuStrip();

        var settingsItem = new ToolStripMenuItem("Open Wellness Companion");
        settingsItem.Click += (_, _) => showSettings();

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => exit();

        menu.Items.Add(settingsItem);
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

        _notifyIcon.DoubleClick += (_, _) => showSettings();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _loadedIcon?.Dispose();
    }
}
