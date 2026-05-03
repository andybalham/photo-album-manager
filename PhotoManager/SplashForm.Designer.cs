namespace PhotoManager;

partial class SplashForm
{
    private System.ComponentModel.IContainer components = null;

    private PictureBox picLogo;
    private Label lblAppName;
    private Label lblTagline;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        picLogo  = new PictureBox();
        lblAppName = new Label();
        lblTagline = new Label();

        ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
        SuspendLayout();

        // Form
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize    = new Size(360, 260);
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor     = Color.FromArgb(22, 68, 85);

        picLogo.Size     = new Size(128, 128);
        picLogo.Location = new Point((ClientSize.Width - 128) / 2, 30);
        picLogo.SizeMode = PictureBoxSizeMode.Zoom;
        picLogo.BackColor = Color.Transparent;

        lblAppName.Text      = "Photo Album Manager";
        lblAppName.ForeColor = Color.FromArgb(240, 255, 252, 245);
        lblAppName.Font      = new Font("Segoe UI", 16f, FontStyle.Bold);
        lblAppName.AutoSize  = false;
        lblAppName.Size      = new Size(ClientSize.Width, 36);
        lblAppName.Location  = new Point(0, 172);
        lblAppName.TextAlign = ContentAlignment.MiddleCenter;
        lblAppName.BackColor = Color.Transparent;

        lblTagline.Text      = "Loading…";
        lblTagline.ForeColor = Color.FromArgb(160, 200, 220, 230);
        lblTagline.Font      = new Font("Segoe UI", 9f);
        lblTagline.AutoSize  = false;
        lblTagline.Size      = new Size(ClientSize.Width, 24);
        lblTagline.Location  = new Point(0, 212);
        lblTagline.TextAlign = ContentAlignment.MiddleCenter;
        lblTagline.BackColor = Color.Transparent;

        Controls.AddRange([picLogo, lblAppName, lblTagline]);

        ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
        ResumeLayout(false);
    }
}
