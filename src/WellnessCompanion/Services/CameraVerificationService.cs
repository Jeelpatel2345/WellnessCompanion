using System.IO;
using OpenCvSharp;

namespace WellnessCompanion.Services;

public sealed class CameraVerificationService : IDisposable
{
    private VideoCapture? _capture;
    private CascadeClassifier? _faceDetector;

    public bool IsRunning { get; private set; }

    public void Start(string cascadePath)
    {
        if (IsRunning)
            return;

        if (!File.Exists(cascadePath))
        {
            throw new FileNotFoundException(
                "Face detection model was not found.",
                cascadePath);
        }

        if (new FileInfo(cascadePath).Length < 1024)
        {
            throw new InvalidDataException(
                "The face detection model is empty or invalid.");
        }

        _capture = new VideoCapture(0);

        if (!_capture.IsOpened())
        {
            _capture.Dispose();
            _capture = null;

            throw new InvalidOperationException(
                "Unable to access the camera. Please check Windows camera permissions and make sure another app is not using the camera.");
        }

        _faceDetector = new CascadeClassifier(cascadePath);

        if (_faceDetector.Empty())
        {
            _capture.Release();
            _capture.Dispose();
            _capture = null;
            _faceDetector.Dispose();
            _faceDetector = null;

            throw new InvalidDataException(
                "The face detection model could not be loaded.");
        }

        IsRunning = true;
    }

    public Mat? CaptureFrame()
    {
        if (!IsRunning || _capture == null)
            return null;

        var frame = new Mat();

        if (!_capture.Read(frame) || frame.Empty())
        {
            frame.Dispose();
            return null;
        }

        return frame;
    }

    public bool FaceDetected(Mat frame)
    {
        if (_faceDetector == null || frame.Empty())
            return false;

        using var gray = new Mat();
        Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
        Cv2.EqualizeHist(gray, gray);

        var faces = _faceDetector.DetectMultiScale(
            gray,
            scaleFactor: 1.1,
            minNeighbors: 5,
            flags: HaarDetectionTypes.ScaleImage,
            minSize: new OpenCvSharp.Size(80, 80));

        return faces.Length > 0;
    }

    public void Stop()
    {
        IsRunning = false;

        _capture?.Release();
        _capture?.Dispose();
        _capture = null;

        _faceDetector?.Dispose();
        _faceDetector = null;
    }

    public void Dispose() => Stop();
}
