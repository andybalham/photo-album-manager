namespace PhotoManager;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private SplitContainer splitContainer;
    private TabControl leftTabControl;
    private TabPage tabSource;
    private TabPage tabTarget;
    private TabPage tabRemoved;
    private TabControl rightTabControl;
    private TabPage tabFileList;
    private TabPage tabPreview;
    private StatusStrip statusStrip;
    internal ToolStripStatusLabel statusLabel;

    // Source tab
    private Button btnSelectSource;
    private Label lblSourcePath;
    private Label lblSourcePlaceholder;

    // Target tab
    private Button btnSelectTarget;
    private Label lblTargetPath;
    private Label lblTargetPlaceholder;

    // Removed tab
    private Label lblRemovedDerived;
    private Label lblRemovedPlaceholder;

    // Right pane placeholders
    private Label lblFileListPlaceholder;
    private Label lblPreviewPlaceholder;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        splitContainer = new SplitContainer();
        leftTabControl = new TabControl();
        tabSource = new TabPage();
        tabTarget = new TabPage();
        tabRemoved = new TabPage();
        rightTabControl = new TabControl();
        tabFileList = new TabPage();
        tabPreview = new TabPage();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();

        btnSelectSource = new Button();
        lblSourcePath = new Label();
        lblSourcePlaceholder = new Label();

        btnSelectTarget = new Button();
        lblTargetPath = new Label();
        lblTargetPlaceholder = new Label();

        lblRemovedDerived = new Label();
        lblRemovedPlaceholder = new Label();

        lblFileListPlaceholder = new Label();
        lblPreviewPlaceholder = new Label();

        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        leftTabControl.SuspendLayout();
        rightTabControl.SuspendLayout();
        SuspendLayout();

        // statusStrip
        statusStrip.Items.AddRange([statusLabel]);
        statusStrip.SizingGrip = false;

        // splitContainer
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.SplitterDistance = 280;
        splitContainer.Panel1.Controls.Add(leftTabControl);
        splitContainer.Panel2.Controls.Add(rightTabControl);

        // leftTabControl
        leftTabControl.Dock = DockStyle.Fill;
        leftTabControl.TabPages.AddRange([tabSource, tabTarget, tabRemoved]);

        // tabSource
        tabSource.Text = "Source";
        tabSource.Padding = new Padding(4);
        ConfigureFolderTab(tabSource, btnSelectSource, lblSourcePath, lblSourcePlaceholder);

        // tabTarget
        tabTarget.Text = "Target";
        tabTarget.Padding = new Padding(4);
        ConfigureFolderTab(tabTarget, btnSelectTarget, lblTargetPath, lblTargetPlaceholder);

        // tabRemoved
        tabRemoved.Text = "Removed";
        tabRemoved.Padding = new Padding(4);
        lblRemovedDerived.Text = "Derived from Target folder";
        lblRemovedDerived.Dock = DockStyle.Top;
        lblRemovedDerived.Height = 24;
        lblRemovedDerived.ForeColor = SystemColors.GrayText;
        lblRemovedPlaceholder.Text = "No folder selected";
        lblRemovedPlaceholder.TextAlign = ContentAlignment.MiddleCenter;
        lblRemovedPlaceholder.Dock = DockStyle.Fill;
        lblRemovedPlaceholder.ForeColor = SystemColors.GrayText;
        tabRemoved.Controls.Add(lblRemovedPlaceholder);
        tabRemoved.Controls.Add(lblRemovedDerived);

        // rightTabControl
        rightTabControl.Dock = DockStyle.Fill;
        rightTabControl.TabPages.AddRange([tabFileList, tabPreview]);

        // tabFileList
        tabFileList.Text = "File List";
        lblFileListPlaceholder.Text = "No folder selected";
        lblFileListPlaceholder.TextAlign = ContentAlignment.MiddleCenter;
        lblFileListPlaceholder.Dock = DockStyle.Fill;
        lblFileListPlaceholder.ForeColor = SystemColors.GrayText;
        tabFileList.Controls.Add(lblFileListPlaceholder);

        // tabPreview
        tabPreview.Text = "Preview";
        lblPreviewPlaceholder.Text = "No image selected";
        lblPreviewPlaceholder.TextAlign = ContentAlignment.MiddleCenter;
        lblPreviewPlaceholder.Dock = DockStyle.Fill;
        lblPreviewPlaceholder.ForeColor = SystemColors.GrayText;
        tabPreview.Controls.Add(lblPreviewPlaceholder);

        // MainForm
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 700);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Photo Album Manager";
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);

        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        leftTabControl.ResumeLayout(false);
        rightTabControl.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private static void ConfigureFolderTab(
        TabPage tab, Button btnSelect, Label lblPath, Label lblPlaceholder)
    {
        btnSelect.Text = "Select Folder…";
        btnSelect.Dock = DockStyle.Top;
        btnSelect.Height = 30;

        lblPath.Dock = DockStyle.Top;
        lblPath.Height = 20;
        lblPath.ForeColor = SystemColors.GrayText;
        lblPath.Font = new Font(SystemFonts.DefaultFont.FontFamily, 8f);

        lblPlaceholder.Text = "No folder selected";
        lblPlaceholder.TextAlign = ContentAlignment.MiddleCenter;
        lblPlaceholder.Dock = DockStyle.Fill;
        lblPlaceholder.ForeColor = SystemColors.GrayText;

        // Add in reverse order so DockStyle.Top stacks correctly
        tab.Controls.Add(lblPlaceholder);
        tab.Controls.Add(lblPath);
        tab.Controls.Add(btnSelect);
    }
}
