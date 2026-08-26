using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using WellnessCompanion.Services;

namespace WellnessCompanion.Windows;

public partial class WaterVerification : System.Windows.Window
{
    private readonly CameraVerificationService _camera;
    private readonly DispatcherTimer _cameraTimer;
    private readonly string _cascadePath;

    public bool Verified { get; private set; }

    public WaterVerification(int attempt = 1, int maxAttempts = 1)
    {
        InitializeComponent();

        AttemptText.Text = maxAttempts > 1
            ? $"Verification attempt {attempt} of {maxAttempts}"
            : "Camera verification";

        _camera = new CameraVerificationService();
        _cascadePath = AssetService.GetFaceCascadePath();

        _cameraTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        _cameraTimer.Tick += CameraTimer_Tick;
        Loaded += WaterVerification_Loaded;
        Closed += WaterVerification_Closed;
    }

    private void WaterVerification_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _camera.Start(_cascadePath);
            StatusText.Text = "Looking for you...";
            _cameraTimer.Start();
        }
        catch (Exception ex)
        {
            StatusText.Text = "Camera unavailable";
            CameraErrorText.Text = ex.Message;
            CameraErrorText.Visibility = Visibility.Visible;
        }
    }

    private void CameraTimer_Tick(object? sender, EventArgs e)
    {
        using var frame = _camera.CaptureFrame();
        if (frame == null)
            return;

        var faceFound = _camera.FaceDetected(frame);

        StatusText.Text = faceFound
            ? "Face detected ✓  You can confirm the break."
            : "Looking for you...";

        ConfirmButton.IsEnabled = faceFound;

        var bitmap = BitmapSourceConverter.ToBitmapSource(frame);
        bitmap.Freeze();
        CameraImage.Source = bitmap;
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        Verified = true;
        Close();
    }

    private void Skip_Click(object sender, RoutedEventArgs e)
    {
        Verified = false;
        Close();
    }

    private void WaterVerification_Closed(object? sender, EventArgs e)
    {
        _cameraTimer.Stop();
        _camera.Stop();
    }
}
