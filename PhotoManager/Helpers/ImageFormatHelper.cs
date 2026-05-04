using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace PhotoManager.Helpers;

public static class ImageFormatHelper
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".heic", ".webp"
    };

    public static bool IsImageFile(string path) =>
        SupportedExtensions.Contains(Path.GetExtension(path));

    public static DateTime GetFileDate(string path)
    {
        var exif = TryGetExifDate(path);
        if (exif.HasValue) return exif.Value;
        var info = new FileInfo(path);
        return info.CreationTime < info.LastWriteTime ? info.CreationTime : info.LastWriteTime;
    }

    // EXIF tag 0x9003 = DateTimeOriginal, 0x0132 = DateTime; format "yyyy:MM:dd HH:mm:ss"
    private static DateTime? TryGetExifDate(string path)
    {
        try
        {
            using var img = Image.FromFile(path);
            foreach (var tagId in new[] { 0x9003, 0x0132 })
            {
                try
                {
                    var prop = img.GetPropertyItem(tagId);
                    if (prop?.Value == null) continue;
                    var raw = Encoding.ASCII.GetString(prop.Value).TrimEnd('\0');
                    if (DateTime.TryParseExact(raw, "yyyy:MM:dd HH:mm:ss",
                            null, System.Globalization.DateTimeStyles.None, out var dt))
                        return dt;
                }
                catch (ArgumentException) { }
            }
        }
        catch { }
        return null;
    }
}
