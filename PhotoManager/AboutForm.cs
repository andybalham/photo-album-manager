using PhotoManager.Helpers;

namespace PhotoManager;

public class AboutForm : Form
{
    public AboutForm(Icon? appIcon)
    {
        Text = "About";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(400, 500);
        Font = new Font("Segoe UI", 9f);

        if (appIcon != null) Icon = appIcon;

        var pic = new PictureBox
        {
            Image = IconHelper.GetAppBitmap(96),
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(96, 96),
            Location = new Point((ClientSize.Width - 96) / 2, 20),
            BackColor = Color.Transparent,
        };

        var lblName = new Label
        {
            Text = "Photo Album Manager",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            AutoSize = false,
            Size = new Size(ClientSize.Width, 30),
            Location = new Point(0, 128),
            TextAlign = ContentAlignment.MiddleCenter,
        };

        var lblVersion = new Label
        {
            Text = $".NET {Environment.Version}",
            ForeColor = SystemColors.GrayText,
            AutoSize = false,
            Size = new Size(ClientSize.Width, 20),
            Location = new Point(0, 160),
            TextAlign = ContentAlignment.MiddleCenter,
        };

        var lblShortcuts = new Label
        {
            Text = """
                Keyboard Shortcuts
                ──────────────────
                F2              Toggle File List / Preview
                F3              Open Album view

                In Preview:
                  ←  /  →       Previous / Next image
                  C             Copy to Target  (Source tab)
                  R             Remove          (Target tab)
                  U             Undo remove     (Removed tab)

                In Album view:
                  ←  ↑  /  →  ↓   Previous / Next image
                  Delete           Remove image
                """,
            Font = new Font("Segoe UI", 9f),
            AutoSize = false,
            Size = new Size(ClientSize.Width - 48, 260),
            Location = new Point(24, 192),
        };

        var btnOk = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Size = new Size(80, 28),
        };
        btnOk.Location = new Point((ClientSize.Width - btnOk.Width) / 2, 462);
        AcceptButton = btnOk;

        Controls.AddRange([pic, lblName, lblVersion, lblShortcuts, btnOk]);
    }
}
