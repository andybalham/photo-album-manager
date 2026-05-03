using PhotoManager.Services;

namespace PhotoManager.Controls;

public partial class FolderTreePanel : UserControl
{
    private readonly FolderScanService _scanService;
    private const string DummyText = "Loading…";

    public event EventHandler<string>? SelectedFolderChanged;

    public string? SelectedFolderPath => treeView.SelectedNode?.Tag as string;

    public FolderTreePanel(FolderScanService scanService)
    {
        _scanService = scanService;
        InitializeComponent();
    }

    public void ShowWarning(string message)
    {
        treeView.Visible = false;
        lblWarning.Text = message;
        lblWarning.Visible = true;
    }

    public void ClearWarning()
    {
        lblWarning.Visible = false;
        treeView.Visible = true;
    }

    public async Task LoadRootAsync(string path, string rootPath)
    {
        treeView.Nodes.Clear();
        if (!Directory.Exists(path)) return;

        var subfolders = await _scanService.GetImageSubfoldersAsync(path);

        var root = new TreeNode(path) { Tag = path };
        foreach (var sub in subfolders)
            root.Nodes.Add(MakeNode(sub));
        root.Expand();

        treeView.BeginUpdate();
        treeView.Nodes.Clear();
        treeView.Nodes.Add(root);
        treeView.EndUpdate();
        root.EnsureVisible();

    }

    public void RemoveFileNode(string folderPath) => _ = RefreshNodeAsync(folderPath);

    public async Task RefreshNodeAsync(string folderPath)
    {
        var node = FindNode(treeView.Nodes, folderPath);
        if (node == null) return;

        var count = await _scanService.GetImageCountAsync(folderPath);
        if (count > 0) return;

        var parent = node.Parent;
        if (parent == null) return; // never remove root node

        node.Remove();

        if (parent.Tag is string parentPath)
            await RefreshNodeAsync(parentPath);
    }

    private static TreeNode? FindNode(TreeNodeCollection nodes, string folderPath)
    {
        foreach (TreeNode node in nodes)
        {
            if (node.Tag is string p && p.Equals(folderPath, StringComparison.OrdinalIgnoreCase))
                return node;
            var found = FindNode(node.Nodes, folderPath);
            if (found != null) return found;
        }
        return null;
    }

    private static TreeNode MakeNode(string folderPath)
    {
        var node = new TreeNode(Path.GetFileName(folderPath)) { Tag = folderPath };
        node.Nodes.Add(new TreeNode(DummyText)); // dummy — Tag is null
        return node;
    }

    private async Task ExpandNodeAsync(TreeNode node)
    {
        if (node.Tag is not string folderPath) return;

        var subfolders = await _scanService.GetImageSubfoldersAsync(folderPath);

        treeView.BeginUpdate();
        node.Nodes.Clear();
        foreach (var sub in subfolders)
            node.Nodes.Add(MakeNode(sub));
        treeView.EndUpdate();
    }

    private async void OnBeforeExpand(object? sender, TreeViewCancelEventArgs e)
    {
        if (e.Node is not { } node) return;
        if (node.Nodes.Count == 1 && node.Nodes[0].Tag is null)
        {
            e.Cancel = true;
            await ExpandNodeAsync(node);
            node.Expand();
        }
    }

    private void OnAfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is string path)
            SelectedFolderChanged?.Invoke(this, path);
    }
}
