using System.Reflection;
using System.IO;
namespace WellnessCompanion.Services;

public static class AssetService
{
    private const string FaceModelFileName = "haarcascade_frontalface_default.xml";

    public static string GetFaceCascadePath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WellnessCompanion",
            "Assets");

        var path = Path.Combine(folder, FaceModelFileName);

        if (File.Exists(path) && new FileInfo(path).Length >= 1024)
            return path;

        Directory.CreateDirectory(folder);

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(FaceModelFileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
            throw new FileNotFoundException("The bundled face detection model is missing from the application.");

        using var input = assembly.GetManifestResourceStream(resourceName);
        if (input == null)
            throw new FileNotFoundException("The bundled face detection model could not be opened.");

        using var output = File.Create(path);
        input.CopyTo(output);

        return path;
    }

    public static Stream? OpenLogoStream()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith("logo.ico", StringComparison.OrdinalIgnoreCase));

        return resourceName == null
            ? null
            : assembly.GetManifestResourceStream(resourceName);
    }
}
