using PhotoManager.Helpers;

namespace PhotoManager;

public partial class SplashForm : Form
{
    public SplashForm()
    {
        InitializeComponent();
        picLogo.Image = IconHelper.GetAppBitmap(128);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(Color.FromArgb(255, 38, 100, 120), 2);
        e.Graphics.DrawRectangle(pen, 1, 1, ClientSize.Width - 2, ClientSize.Height - 2);
    }
}
