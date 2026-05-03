namespace PhotoManager;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 600);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Photo Album Manager";
    }
}
