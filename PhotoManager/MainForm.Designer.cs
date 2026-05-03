using PhotoManager.Controls;

namespace PhotoManager;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private SplitContainer splitContainer;
    private TabControl leftTabControl;
    internal TabPage tabSource;
    internal TabPage tabTarget;
    internal TabPage tabRemoved;
    private TabControl rightTabControl;
    internal TabPage tabFileList;
    internal TabPage tabPreview;
    private StatusStrip statusStrip;
    internal ToolStripStatusLabel statusLabel;

    // Source tab header controls
    private Button btnSelectSource;
    internal Label lblSourcePath;

    // Target tab header controls
    private Button btnSelectTarget;
    internal Label lblTargetPath;

    // Removed tab header
    private Label lblRemovedDerived;

    // Right pane placeholders (replaced in later phases)
    internal Label lblFileListPlaceholder;
    internal Label lblPreviewPlaceholder;

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

        btnSelectTarget = new Button();
        lblTargetPath = new Label();

        lblRemovedDerived = new Label();

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

        // --- Source tab ---
        tabSource.Text = "Source";
        tabSource.Padding = new Padding(4);
        BuildFolderTabHeader(tabSource, btnSelectSource, lblSourcePath);

        // --- Target tab ---
        tabTarget.Text = "Target";
        tabTarget.Padding = new Padding(4);
        BuildFolderTabHeader(tabTarget, btnSelectTarget, lblTargetPath);

        // --- Removed tab ---
        tabRemoved.Text = "Removed";
        tabRemoved.Padding = new Padding(4);
        lblRemovedDerived.Text = "Derived from Target folder";
        lblRemovedDerived.Dock = DockStyle.Top;
        lblRemovedDerived.Height = 24;
        lblRemovedDerived.ForeColor = SystemColors.GrayText;
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

    private static void BuildFolderTabHeader(TabPage tab, Button btnSelect, Label lblPath)
    {
        btnSelect.Text = "Select Folder…";
        btnSelect.Dock = DockStyle.Top;
        btnSelect.Height = 30;

        lblPath.Dock = DockStyle.Top;
        lblPath.Height = 20;
        lblPath.ForeColor = SystemColors.GrayText;
        lblPath.Font = new Font(SystemFonts.DefaultFont.FontFamily, 8f);

        // Controls added last appear on top with DockStyle.Top
        tab.Controls.Add(lblPath);
        tab.Controls.Add(btnSelect);
    }
}
