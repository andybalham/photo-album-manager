using PhotoManager.Controls;
using PhotoManager.Models;
using PhotoManager.Services;
using PhotoManager.Settings;

namespace PhotoManager;

public partial class MainForm : Form
{
    private readonly AppSettings _settings;
    private readonly FolderScanService _scanService;
    private readonly FileOperationService _fileOpService;
    private readonly ImageLoadService _imageService;

    private readonly FolderTreePanel _sourceTree;
    private readonly FolderTreePanel _targetTree;
    private readonly FolderTreePanel _removedTree;
    private readonly FileListPanel _fileListPanel;
    private readonly PreviewPanel _previewPanel;

    // Tracks which folder panel last fired SelectedFolderChanged
    private string _activeFolderPath = string.Empty;
    private FolderTreePanel? _activeTree;

    public MainForm(AppSettings settings)
    {
        _settings = settings;
        _scanService = new FolderScanService();
        _fileOpService = new FileOperationService();
        _imageService = new ImageLoadService();

        InitializeComponent();

        _sourceTree = new FolderTreePanel(_scanService);
        _targetTree = new FolderTreePanel(_scanService);
        _removedTree = new FolderTreePanel(_scanService);
        _fileListPanel = new FileListPanel(_scanService);
        _previewPanel = new PreviewPanel(_imageService, _fileOpService);

        tabSource.Controls.Add(_sourceTree);
        tabTarget.Controls.Add(_targetTree);
        tabRemoved.Controls.Add(_removedTree);

        tabFileList.Controls.Remove(lblFileListPlaceholder);
        tabFileList.Controls.Add(_fileListPanel);

        tabPreview.Controls.Remove(lblPreviewPlaceholder);
        tabPreview.Controls.Add(_previewPanel);

        _sourceTree.SelectedFolderChanged += OnFolderSelected;
        _targetTree.SelectedFolderChanged += OnFolderSelected;
        _removedTree.SelectedFolderChanged += OnFolderSelected;

        _fileListPanel.SortChanged += OnSortChanged;
        _fileListPanel.FileSelected += OnFileSelected;
        _fileListPanel.FileDoubleClicked += OnFileDoubleClicked;

        _previewPanel.SortChanged += OnPreviewSortChanged;
        _previewPanel.FileActioned += OnFileActioned;

        btnSelectSource.Click += OnSelectSource;
        btnSelectTarget.Click += OnSelectTarget;
        splitContainer.SplitterMoved += OnSplitterMoved;
        Load += OnLoad;
    }

    // ── Startup ───────────────────────────────────────────────────────────────

    private async void OnLoad(object? sender, EventArgs e)
    {
        splitContainer.SplitterDistance = _settings.SplitterPosition;

        if (!string.IsNullOrEmpty(_settings.SourceFolderPath))
        {
            lblSourcePath.Text = _settings.SourceFolderPath;
            if (Directory.Exists(_settings.SourceFolderPath))
                await _sourceTree.LoadRootAsync(_settings.SourceFolderPath, _settings.SourceFolderPath);
        }

        if (!string.IsNullOrEmpty(_settings.TargetFolderPath))
        {
            lblTargetPath.Text = _settings.TargetFolderPath;
            if (Directory.Exists(_settings.TargetFolderPath))
            {
                await _targetTree.LoadRootAsync(_settings.TargetFolderPath, _settings.TargetFolderPath);
                await LoadRemovedTreeAsync(_settings.TargetFolderPath);
            }
        }
    }

    // ── Folder selection ─────────────────────────────────────────────────────

    private async void OnSelectSource(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog();
        if (dlg.ShowDialog() != DialogResult.OK) return;
        _settings.SourceFolderPath = dlg.SelectedPath;
        lblSourcePath.Text = dlg.SelectedPath;
        await _sourceTree.LoadRootAsync(dlg.SelectedPath, dlg.SelectedPath);
    }

    private async void OnSelectTarget(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog();
        if (dlg.ShowDialog() != DialogResult.OK) return;
        _settings.TargetFolderPath = dlg.SelectedPath;
        lblTargetPath.Text = dlg.SelectedPath;
        await _targetTree.LoadRootAsync(dlg.SelectedPath, dlg.SelectedPath);
        await LoadRemovedTreeAsync(dlg.SelectedPath);
    }

    private async Task LoadRemovedTreeAsync(string targetRoot)
    {
        var removedRoot = Path.Combine(targetRoot, "_removed");
        if (Directory.Exists(removedRoot))
            await _removedTree.LoadRootAsync(removedRoot, removedRoot);
        else
            _removedTree.treeView.Nodes.Clear();
    }

    // ── Tree → file list ──────────────────────────────────────────────────────

    private async void OnFolderSelected(object? sender, string folderPath)
    {
        _activeFolderPath = folderPath;
        _activeTree = sender as FolderTreePanel;

        var (_, sourceRoot, targetRoot) = GetContext();
        var sort = new SortOptions(_settings.SortField, _settings.SortDirection);
        await _fileListPanel.LoadFolderAsync(folderPath, sourceRoot, sort);

        var count = await _scanService.GetImageCountAsync(folderPath);
        statusLabel.Text = $"{count} image(s)";
    }

    // ── File list → preview ───────────────────────────────────────────────────

    private void OnFileSelected(object? sender, ImageFile file)
    {
        if (rightTabControl.SelectedTab == tabPreview)
            _ = OpenPreviewAsync(file);
    }

    private void OnFileDoubleClicked(object? sender, ImageFile file)
    {
        rightTabControl.SelectedTab = tabPreview;
        _ = OpenPreviewAsync(file);
    }

    private async Task OpenPreviewAsync(ImageFile selectedFile)
    {
        var files = _fileListPanel.GetCurrentFiles();
        var idx = files.ToList().IndexOf(selectedFile);
        if (idx < 0) idx = 0;

        var (context, sourceRoot, targetRoot) = GetContext();
        await _previewPanel.LoadFolderAsync(files, idx,
            _fileListPanel.CurrentSort, context, sourceRoot, targetRoot);
    }

    // ── Sort sync ─────────────────────────────────────────────────────────────

    private void OnSortChanged(object? sender, SortOptions sort)
    {
        _settings.SortField = sort.Field;
        _settings.SortDirection = sort.Direction;
    }

    private void OnPreviewSortChanged(object? sender, SortOptions sort)
    {
        _settings.SortField = sort.Field;
        _settings.SortDirection = sort.Direction;
        _fileListPanel.SetSort(sort);
    }

    // ── File actioned ─────────────────────────────────────────────────────────

    private async void OnFileActioned(object? sender, ImageFile file)
    {
        _fileListPanel.RemoveFile(file);

        if (_activeTree != null)
            await _activeTree.RefreshNodeAsync(_activeFolderPath);

        // Refresh removed tree after any operation that affects it
        if (!string.IsNullOrEmpty(_settings.TargetFolderPath))
            await LoadRemovedTreeAsync(_settings.TargetFolderPath);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private (PreviewContext context, string sourceRoot, string targetRoot) GetContext()
    {
        if (leftTabControl.SelectedTab == tabSource)
            return (PreviewContext.Source, _settings.SourceFolderPath, _settings.TargetFolderPath);
        if (leftTabControl.SelectedTab == tabTarget)
            return (PreviewContext.Target, _settings.TargetFolderPath, _settings.TargetFolderPath);
        return (PreviewContext.Removed,
            Path.Combine(_settings.TargetFolderPath, "_removed"),
            _settings.TargetFolderPath);
    }

    private void OnSplitterMoved(object? sender, SplitterEventArgs e)
    {
        _settings.SplitterPosition = splitContainer.SplitterDistance;
    }
}
