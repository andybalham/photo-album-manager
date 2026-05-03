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

    private readonly FolderTreePanel _sourceTree;
    private readonly FolderTreePanel _targetTree;
    private readonly FolderTreePanel _removedTree;
    private readonly FileListPanel _fileListPanel;

    public MainForm(AppSettings settings)
    {
        _settings = settings;
        _scanService = new FolderScanService();
        _fileOpService = new FileOperationService();

        InitializeComponent();

        _sourceTree = new FolderTreePanel(_scanService);
        _targetTree = new FolderTreePanel(_scanService);
        _removedTree = new FolderTreePanel(_scanService);
        _fileListPanel = new FileListPanel(_scanService);

        tabSource.Controls.Add(_sourceTree);
        tabTarget.Controls.Add(_targetTree);
        tabRemoved.Controls.Add(_removedTree);

        // Replace file list placeholder with real panel
        tabFileList.Controls.Remove(lblFileListPlaceholder);
        tabFileList.Controls.Add(_fileListPanel);

        _sourceTree.SelectedFolderChanged += OnFolderSelected;
        _targetTree.SelectedFolderChanged += OnFolderSelected;
        _removedTree.SelectedFolderChanged += OnFolderSelected;

        _fileListPanel.SortChanged += OnSortChanged;

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

    // ── Tree selection → file list ────────────────────────────────────────────

    private async void OnFolderSelected(object? sender, string folderPath)
    {
        var rootPath = sender == _sourceTree ? _settings.SourceFolderPath
                     : sender == _targetTree ? _settings.TargetFolderPath
                     : Path.Combine(_settings.TargetFolderPath, "_removed");

        var sort = new SortOptions(_settings.SortField, _settings.SortDirection);
        await _fileListPanel.LoadFolderAsync(folderPath, rootPath, sort);

        var count = await _scanService.GetImageCountAsync(folderPath);
        statusLabel.Text = $"{count} image(s)";
    }

    // ── Sort persistence ──────────────────────────────────────────────────────

    private void OnSortChanged(object? sender, SortOptions sort)
    {
        _settings.SortField = sort.Field;
        _settings.SortDirection = sort.Direction;
    }

    // ── Splitter ──────────────────────────────────────────────────────────────

    private void OnSplitterMoved(object? sender, SplitterEventArgs e)
    {
        _settings.SplitterPosition = splitContainer.SplitterDistance;
    }
}
