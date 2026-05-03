namespace PhotoManager.Controls;

partial class FolderTreePanel
{
    private System.ComponentModel.IContainer components = null;
    internal TreeView treeView;

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
        SuspendLayout();

        treeView.Dock = DockStyle.Fill;
        treeView.CheckBoxes = false;
        treeView.HideSelection = false;
        treeView.BeforeExpand += OnBeforeExpand;
        treeView.AfterSelect += OnAfterSelect;

        Controls.Add(treeView);
        Dock = DockStyle.Fill;
        ResumeLayout(false);
    }
}
