namespace PhotoManager.Controls;

partial class FolderTreePanel
{
    private System.ComponentModel.IContainer components = null;
    internal TreeView treeView;
    private Label lblWarning;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        treeView = new TreeView();
        lblWarning = new Label();
        SuspendLayout();

        treeView.Dock = DockStyle.Fill;
        treeView.CheckBoxes = false;
        treeView.HideSelection = false;
        treeView.BeforeExpand += OnBeforeExpand;
        treeView.AfterSelect += OnAfterSelect;

        lblWarning.Dock = DockStyle.Fill;
        lblWarning.TextAlign = ContentAlignment.MiddleCenter;
        lblWarning.ForeColor = Color.Firebrick;
        lblWarning.Visible = false;

        Controls.Add(treeView);
        Controls.Add(lblWarning);
        Dock = DockStyle.Fill;
        ResumeLayout(false);
    }
}
