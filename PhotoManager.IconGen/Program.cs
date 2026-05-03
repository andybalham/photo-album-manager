using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

var outPath = args.Length > 0 ? args[0] : "app.ico";

using var bmp256 = Draw(256);
using var bmp64  = Draw(64);
using var bmp32  = Draw(32);
using var bmp16  = Draw(16);

WriteIco(outPath, [bmp256, bmp64, bmp32, bmp16]);
Console.WriteLine($"Icon written to {outPath}");

static Bitmap Draw(int size)
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

static void WriteIco(string path, Bitmap[] bitmaps)
{
    var images = bitmaps.Select(b => { var ms = new MemoryStream(); b.Save(ms, ImageFormat.Png); return ms.ToArray(); }).ToArray();
    using var w = new BinaryWriter(File.Create(path));
    w.Write((short)0); w.Write((short)1); w.Write((short)bitmaps.Length);
    int offset = 6 + bitmaps.Length * 16;
    for (int i = 0; i < bitmaps.Length; i++)
    {
        int sz = bitmaps[i].Width;
        w.Write((byte)(sz >= 256 ? 0 : sz));
        w.Write((byte)(sz >= 256 ? 0 : sz));
        w.Write((byte)0); w.Write((byte)0);
        w.Write((short)1); w.Write((short)32);
        w.Write(images[i].Length); w.Write(offset);
        offset += images[i].Length;
    }
    foreach (var img in images) w.Write(img);
}
