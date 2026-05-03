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

        var sourceTreePanel = new Panel { Dock = DockStyle.Fill };
        sourceTreePanel.Controls.Add(_sourceTree);
        tabSource.Controls.Add(sourceTreePanel);
        BuildFolderTabHeader(tabSource, btnSelectSource);

        var targetTreePanel = new Panel { Dock = DockStyle.Fill };
        targetTreePanel.Controls.Add(_targetTree);
        tabTarget.Controls.Add(targetTreePanel);
        BuildFolderTabHeader(tabTarget, btnSelectTarget);

        var removedTreePanel = new Panel { Dock = DockStyle.Fill };
        removedTreePanel.Controls.Add(_removedTree);
        tabRemoved.Controls.Add(removedTreePanel);

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
        _fileListPanel.NameColumnWidthChanged += (_, w) => _settings.NameColumnWidth = w;

        _previewPanel.SortChanged += OnPreviewSortChanged;
        _previewPanel.FileActioned += OnFileActioned;
        _previewPanel.FileChanged += OnPreviewFileChanged;
        _previewPanel.StatusMessage += OnStatusMessage;

        btnSelectSource.Click += OnSelectSource;
        btnSelectTarget.Click += OnSelectTarget;
        leftTabControl.SelectedIndexChanged += OnLeftTabChanged;
        splitContainer.SplitterMoved += OnSplitterMoved;
        Resize += OnFormResize;
        Move += OnFormMove;
        Load += OnLoad;
    }

    // ── Startup ───────────────────────────────────────────────────────────────

    private async void OnLoad(object? sender, EventArgs e)
    {
        if (_settings.WindowLeft >= 0 && _settings.WindowTop >= 0)
        {
            Left = _settings.WindowLeft;
            Top = _settings.WindowTop;
        }
        if (_settings.WindowWidth > 0) Width = _settings.WindowWidth;
        if (_settings.WindowHeight > 0) Height = _settings.WindowHeight;
        if (Enum.TryParse<FormWindowState>(_settings.WindowState, out var ws) && ws != FormWindowState.Minimized)
            WindowState = ws;

        splitContainer.SplitterDistance = _settings.SplitterPosition;

        if (_settings.NameColumnWidth > 0)
            _fileListPanel.SetNameColumnWidth(_settings.NameColumnWidth);

        if (!string.IsNullOrEmpty(_settings.SourceFolderPath))
        {
            if (Directory.Exists(_settings.SourceFolderPath))
                await _sourceTree.LoadRootAsync(_settings.SourceFolderPath, _settings.SourceFolderPath);
            else
                _sourceTree.ShowWarning("Folder not found — please select again");
        }

        if (!string.IsNullOrEmpty(_settings.TargetFolderPath))
        {
            if (Directory.Exists(_settings.TargetFolderPath))
            {
                await _targetTree.LoadRootAsync(_settings.TargetFolderPath, _settings.TargetFolderPath);
                await LoadRemovedTreeAsync(_settings.TargetFolderPath);
            }
            else
                _targetTree.ShowWarning("Folder not found — please select again");
        }
    }

    // ── Folder selection ─────────────────────────────────────────────────────

    private async void OnSelectSource(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog();
        if (dlg.ShowDialog() != DialogResult.OK) return;
        _settings.SourceFolderPath = dlg.SelectedPath;
        _sourceTree.ClearWarning();
        await _sourceTree.LoadRootAsync(dlg.SelectedPath, dlg.SelectedPath);
    }

    private async void OnSelectTarget(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog();
        if (dlg.ShowDialog() != DialogResult.OK) return;
        _settings.TargetFolderPath = dlg.SelectedPath;
        _targetTree.ClearWarning();
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

        var (_, sourceRoot, _) = GetContext();
        var sort = new SortOptions(_settings.SortField, _settings.SortDirection);
        await _fileListPanel.LoadFolderAsync(folderPath, sourceRoot, sort);

        rightTabControl.SelectedTab = tabFileList;

        var files = _fileListPanel.GetCurrentFiles().ToList();
        if (files.Count > 0)
        {
            _fileListPanel.SelectFirst();
            await OpenPreviewAsync(files[0]);
        }
        else
            _previewPanel.Clear();

        var count = await _scanService.GetImageCountAsync(folderPath);
        statusLabel.Text = $"{count} image(s)";
    }

    // ── Left tab switch ───────────────────────────────────────────────────────

    private async void OnLeftTabChanged(object? sender, EventArgs e)
    {
        var tree = leftTabControl.SelectedTab == tabSource ? _sourceTree
                 : leftTabControl.SelectedTab == tabTarget ? _targetTree
                 : _removedTree;

        var folderPath = tree.SelectedFolderPath;
        if (string.IsNullOrEmpty(folderPath))
        {
            _fileListPanel.ClearFiles();
            _previewPanel.Clear();
            return;
        }

        _activeFolderPath = folderPath;
        _activeTree = tree;

        var (_, sourceRoot, _) = GetContext();
        var sort = new SortOptions(_settings.SortField, _settings.SortDirection);
        await _fileListPanel.LoadFolderAsync(folderPath, sourceRoot, sort);

        rightTabControl.SelectedTab = tabFileList;

        var files = _fileListPanel.GetCurrentFiles().ToList();
        if (files.Count > 0)
        {
            _fileListPanel.SelectFirst();
            await OpenPreviewAsync(files[0]);
        }
        else
            _previewPanel.Clear();
    }

    // ── File list → preview ───────────────────────────────────────────────────

    private void OnFileSelected(object? sender, ImageFile file)
    {
        if (_syncingSelection) return;
        _ = OpenPreviewAsync(file);
    }

    private void OnFileDoubleClicked(object? sender, ImageFile file)
    {
        rightTabControl.SelectedTab = tabPreview;
        _ = OpenPreviewAsync(file);
    }

    private bool _syncingSelection;

    private void OnPreviewFileChanged(object? sender, ImageFile file)
    {
        if (_syncingSelection) return;
        _syncingSelection = true;
        _fileListPanel.SelectFile(file);
        _syncingSelection = false;
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

        // Copy (source→target) or undo (removed→target): reload target tree + file list
        var context = _previewPanel.Context;
        if ((context == PreviewContext.Source || context == PreviewContext.Removed)
            && !string.IsNullOrEmpty(_settings.TargetFolderPath))
        {
            await _targetTree.LoadRootAsync(_settings.TargetFolderPath, _settings.TargetFolderPath);

            if (leftTabControl.SelectedTab == tabTarget)
            {
                var folderPath = _targetTree.SelectedFolderPath;
                if (!string.IsNullOrEmpty(folderPath))
                {
                    var sort = new SortOptions(_settings.SortField, _settings.SortDirection);
                    await _fileListPanel.LoadFolderAsync(folderPath, _settings.TargetFolderPath, sort);
                }
            }
        }
    }

    // ── Window persistence ────────────────────────────────────────────────────

    private void OnFormResize(object? sender, EventArgs e) => SaveWindowBounds();
    private void OnFormMove(object? sender, EventArgs e) => SaveWindowBounds();

    private void SaveWindowBounds()
    {
        _settings.WindowState = WindowState.ToString();
        if (WindowState == FormWindowState.Normal)
        {
            _settings.WindowWidth = Width;
            _settings.WindowHeight = Height;
            _settings.WindowLeft = Left;
            _settings.WindowTop = Top;
        }
    }

    // ── Keyboard shortcuts ────────────────────────────────────────────────────

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (rightTabControl.SelectedTab == tabPreview)
        {
            switch (keyData)
            {
                case Keys.Left:  _previewPanel.NavigatePrev(); return true;
                case Keys.Right: _previewPanel.NavigateNext(); return true;
                case Keys.C when _previewPanel.Context == PreviewContext.Source && _previewPanel.CanAction:
                    _previewPanel.TriggerAction(); return true;
                case Keys.R when _previewPanel.Context == PreviewContext.Target && _previewPanel.CanAction:
                    _previewPanel.TriggerAction(); return true;
                case Keys.U when _previewPanel.Context == PreviewContext.Removed && _previewPanel.CanAction:
                    _previewPanel.TriggerAction(); return true;
            }
        }
        if (keyData == Keys.F2)
        {
            rightTabControl.SelectedTab =
                rightTabControl.SelectedTab == tabFileList ? tabPreview : tabFileList;
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    // ── About ─────────────────────────────────────────────────────────────────

    private void OnAboutClick(object? sender, EventArgs e)
    {
        var version = Environment.Version;
        MessageBox.Show(
            $"""
            Photo Album Manager  ·  .NET {version}

            Keyboard Shortcuts
            ──────────────────
            F2              Toggle File List / Preview

            In Preview:
              ←  /  →       Previous / Next image
              C             Copy to Target  (Source tab)
              R             Remove          (Target tab)
              U             Undo remove     (Removed tab)
            """,
            "About",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    // ── Status messages ───────────────────────────────────────────────────────

    private CancellationTokenSource? _statusCts;

    private async void OnStatusMessage(object? sender, string message)
    {
        _statusCts?.Cancel();
        _statusCts = new CancellationTokenSource();
        var token = _statusCts.Token;

        statusLabel.Text = message;
        try
        {
            await Task.Delay(3000, token);
            statusLabel.Text = string.Empty;
        }
        catch (OperationCanceledException) { }
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
