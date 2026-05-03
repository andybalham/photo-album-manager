namespace PhotoManager.Helpers;

public static class ImageFormatHelper
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".heic", ".webp"
    };

    public static bool IsImageFile(string path) =>
        SupportedExtensions.Contains(Path.GetExtension(path));
}
