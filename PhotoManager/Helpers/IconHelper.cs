using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;

namespace PhotoManager.Helpers;

public static class IconHelper
{
    private static Icon? _cachedIcon;

    public static Icon GetAppIcon()
    {
        if (_cachedIcon != null) return _cachedIcon;

        var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("PhotoManager.Resources.app.ico");

        if (stream != null)
        {
            _cachedIcon = new Icon(stream);
            return _cachedIcon;
        }

        // Fallback: generate at runtime
        _cachedIcon = CreateAlbumIcon();
        return _cachedIcon;
    }

    public static Bitmap GetAppBitmap(int size)
    {
        try
        {
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("PhotoManager.Resources.app.ico");
            if (stream != null)
            {
                using var icon = new Icon(stream, new Size(size, size));
                return icon.ToBitmap();
            }
        }
        catch { }

        return CreateAlbumBitmap(size);
    }

    public static Bitmap CreateAlbumBitmap(int size)
    {
        var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        float s = size;
        float spineW = s * 0.12f;
        var cover = new RectangleF(spineW, s * 0.06f, s * 0.84f, s * 0.88f);

        using var coverBrush = new LinearGradientBrush(
            new PointF(cover.Left, cover.Top), new PointF(cover.Right, cover.Bottom),
            Color.FromArgb(255, 38, 100, 120), Color.FromArgb(255, 22, 68, 85));
        g.FillRectangle(coverBrush, cover);

        var spine = new RectangleF(0, s * 0.06f, spineW, s * 0.88f);
        using var spineBrush = new SolidBrush(Color.FromArgb(255, 18, 55, 70));
        g.FillRectangle(spineBrush, spine);

        using var pagePen = new Pen(Color.FromArgb(180, 220, 220, 210), s * 0.015f);
        for (int i = 1; i <= 3; i++)
        {
            float o = i * s * 0.018f;
            g.DrawLine(pagePen, cover.Right + o, cover.Top + o * 2, cover.Right + o, cover.Bottom - o);
        }

        float fm = s * 0.14f;
        var photoFrame = new RectangleF(cover.Left + fm, cover.Top + fm, cover.Width - fm * 2, cover.Height - fm * 3.2f);
        using var frameBrush = new SolidBrush(Color.FromArgb(240, 255, 252, 245));
        g.FillRectangle(frameBrush, photoFrame);

        var sky = new RectangleF(photoFrame.X, photoFrame.Y, photoFrame.Width, photoFrame.Height * 0.55f);
        using var skyBrush = new LinearGradientBrush(sky,
            Color.FromArgb(255, 135, 196, 235), Color.FromArgb(255, 180, 220, 250), LinearGradientMode.Vertical);
        g.FillRectangle(skyBrush, sky);

        var mountain = new[]
        {
            new PointF(photoFrame.Left, photoFrame.Bottom),
            new PointF(photoFrame.Left + photoFrame.Width * 0.25f, photoFrame.Top + photoFrame.Height * 0.40f),
            new PointF(photoFrame.Left + photoFrame.Width * 0.50f, photoFrame.Top + photoFrame.Height * 0.60f),
            new PointF(photoFrame.Left + photoFrame.Width * 0.70f, photoFrame.Top + photoFrame.Height * 0.35f),
            new PointF(photoFrame.Right, photoFrame.Bottom),
        };
        using var mountainBrush = new SolidBrush(Color.FromArgb(255, 75, 115, 90));
        g.FillPolygon(mountainBrush, mountain);

        var ground = new RectangleF(photoFrame.X, photoFrame.Bottom - photoFrame.Height * 0.20f,
            photoFrame.Width, photoFrame.Height * 0.20f);
        using var groundBrush = new SolidBrush(Color.FromArgb(255, 100, 145, 100));
        g.FillRectangle(groundBrush, ground);

        float labelH = s * 0.12f;
        var label = new RectangleF(cover.Left, cover.Bottom - labelH, cover.Width, labelH);
        using var labelBrush = new SolidBrush(Color.FromArgb(80, 255, 255, 255));
        g.FillRectangle(labelBrush, label);

        using var borderPen = new Pen(Color.FromArgb(255, 15, 45, 60), s * 0.02f);
        g.DrawRectangle(borderPen, cover.X, cover.Y, cover.Width, cover.Height);

        return bmp;
    }

    private static Icon CreateAlbumIcon()
    {
        using var bmp32 = CreateAlbumBitmap(32);
        using var bmp16 = CreateAlbumBitmap(16);
        return CombineToIcon([bmp32, bmp16]);
    }

    private static Icon CombineToIcon(Bitmap[] bitmaps)
    {
        using var ms = new MemoryStream();
        WriteIconStream(ms, bitmaps);
        ms.Seek(0, SeekOrigin.Begin);
        return new Icon(ms);
    }

    private static void WriteIconStream(Stream stream, Bitmap[] bitmaps)
    {
        var writer = new BinaryWriter(stream);
        int count = bitmaps.Length;
        writer.Write((short)0); writer.Write((short)1); writer.Write((short)count);

        var imageData = bitmaps.Select(b => { using var ms = new MemoryStream(); b.Save(ms, ImageFormat.Png); return ms.ToArray(); }).ToArray();

        int dataOffset = 6 + count * 16;
        for (int i = 0; i < count; i++)
        {
            int sz = bitmaps[i].Width;
            writer.Write((byte)(sz == 256 ? 0 : sz));
            writer.Write((byte)(sz == 256 ? 0 : sz));
            writer.Write((byte)0); writer.Write((byte)0);
            writer.Write((short)1); writer.Write((short)32);
            writer.Write(imageData[i].Length); writer.Write(dataOffset);
            dataOffset += imageData[i].Length;
        }
        foreach (var data in imageData) writer.Write(data);
    }
}
