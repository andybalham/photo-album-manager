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

    public async Task LoadRootAsync(string path, string rootPath)
    {
        treeView.Nodes.Clear();
        if (!Directory.Exists(path)) return;

        var node = MakeNode(path);
        treeView.BeginUpdate();
        treeView.Nodes.Add(node);
        treeView.EndUpdate();

        await ExpandNodeAsync(node);
        node.Expand();
    }

    public void RemoveFileNode(string folderPath)
    {
        // Phase 8
    }

    public async Task RefreshNodeAsync(string folderPath)
    {
        // Phase 8
        await Task.CompletedTask;
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
