namespace PhotoManager.Services;

public class ImageLoadService
{
    public async Task<(Bitmap? Bitmap, string? ErrorMessage)> LoadAsync(string path, Size displaySize)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var src = Image.FromFile(path);
                var target = ScaleToFit(src.Size, displaySize);
                var bmp = new Bitmap(target.Width, target.Height);
                using var g = Graphics.FromImage(bmp);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(src, 0, 0, target.Width, target.Height);
                return ((Bitmap?)bmp, (string?)null);
            }
            catch (OutOfMemoryException)
            {
                var msg = Path.GetExtension(path).Equals(".heic", StringComparison.OrdinalIgnoreCase)
                    ? "HEIC preview requires Windows HEIC codec"
                    : "Cannot preview this image";
                return (null, msg);
            }
            catch
            {
                return (null, "Cannot preview this image");
            }
        });
    }

    private static Size ScaleToFit(Size source, Size display)
    {
        if (display.Width <= 0 || display.Height <= 0 || source.Width <= 0 || source.Height <= 0)
            return source;
        var ratio = Math.Min((double)display.Width / source.Width, (double)display.Height / source.Height);
        return new Size(Math.Max(1, (int)(source.Width * ratio)), Math.Max(1, (int)(source.Height * ratio)));
    }
}
