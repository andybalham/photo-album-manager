using PhotoManager.Models;
using PhotoManager.Services;

namespace PhotoManager.Controls;

public partial class FileListPanel : UserControl
{
    private readonly FolderScanService _scanService;
    private List<ImageFile> _files = [];
    private string _currentFolder = string.Empty;
    private string _currentRoot = string.Empty;
    private int _savedNameColumnWidth = 0;
    private CancellationTokenSource? _loadCts;
    private int _loadGeneration = 0;

    public SortOptions CurrentSort { get; private set; } = SortOptions.Default;

    public ImageFile? SelectedFile =>
        listView.SelectedItems.Count > 0
            ? listView.SelectedItems[0].Tag as ImageFile
            : null;

    public event EventHandler<ImageFile>? FileSelected;
    public event EventHandler<ImageFile>? FileDoubleClicked;

    public FileListPanel(FolderScanService scanService)
    {
        _scanService = scanService;
        InitializeComponent();
        Resize += (_, _) => ResizeNameColumn();
    }

    public async Task LoadFolderAsync(string folderPath, string rootPath, SortOptions sort,
        IReadOnlyList<string>? excludeRoots = null)
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var ct = _loadCts.Token;
        var generation = ++_loadGeneration;

        _currentFolder = folderPath;
        _currentRoot = rootPath;
        CurrentSort = sort;
        _files = [];
        listView.Items.Clear();

        listView.Enabled = false;
        Cursor = Cursors.WaitCursor;
        lblStatus.Text = "Loading...";
        lblStatus.Visible = true;
        progressBar.Visible = true;

        var collected = new List<ImageFile>();
        try
        {
            int count = 0;
            await foreach (var file in _scanService.StreamFilesInFolderAsync(folderPath, rootPath, excludeRoots, ct))
            {
                collected.Add(file);
                count++;
                if (count % 10 == 0)
                    lblStatus.Text = $"Loading... ({count})";
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        finally
        {
            if (generation == _loadGeneration)
            {
                listView.Enabled = true;
                Cursor = Cursors.Default;
                progressBar.Visible = false;
                lblStatus.Visible = false;
            }
        }

        if (generation != _loadGeneration) return;

        _files = collected;
        PopulateList();
        UpdateSortButtons();
    }

    public void SetSort(SortOptions sort)
    {
        CurrentSort = sort;
        PopulateList();
        UpdateSortButtons();
    }

    public IReadOnlyList<ImageFile> GetCurrentFiles() =>
        listView.Items.Cast<ListViewItem>()
            .Select(i => (ImageFile)i.Tag!)
            .ToList();

    public void RemoveFile(ImageFile file)
    {
        _files.Remove(file);
        PopulateList();
    }

    public void SelectFirst()
    {
        if (listView.Items.Count == 0) return;
        listView.Items[0].Selected = true;
        listView.Items[0].Focused = true;
        listView.EnsureVisible(0);
    }

    public void SelectFile(ImageFile file)
    {
        foreach (ListViewItem item in listView.Items)
        {
            if (item.Tag is ImageFile f && f.FullPath == file.FullPath)
            {
                listView.SelectedItems.Clear();
                item.Selected = true;
                item.Focused = true;
                listView.EnsureVisible(item.Index);
                return;
            }
        }
    }

    public void ClearFiles()
    {
        _files = [];
        PopulateList();
    }

    private void PopulateList()
    {
        IEnumerable<ImageFile> sorted = CurrentSort.Field == SortField.DateCreated
            ? _files.OrderBy(f => f.DateModified)
            : _files.OrderBy(f => f.FileName, StringComparer.OrdinalIgnoreCase);

        if (CurrentSort.Direction == SortDirection.Descending)
            sorted = sorted.Reverse();

        listView.BeginUpdate();
        listView.Items.Clear();
        foreach (var file in sorted)
        {
            var item = new ListViewItem(file.FileName) { Tag = file };
            item.SubItems.Add(file.FormattedDate);
            item.SubItems.Add(file.FormattedSize);
            listView.Items.Add(item);
        }
        listView.EndUpdate();

        ResizeNameColumn();
    }

    private void UpdateSortButtons()
    {
        var nameDir = CurrentSort.Field == SortField.Name
            ? (CurrentSort.Direction == SortDirection.Ascending ? " ▲" : " ▼")
            : string.Empty;
        var dateDir = CurrentSort.Field == SortField.DateCreated
            ? (CurrentSort.Direction == SortDirection.Ascending ? " ▲" : " ▼")
            : string.Empty;

        btnSortName.Text = $"Sort by Name{nameDir}";
        btnSortDate.Text = $"Sort by Date{dateDir}";
        btnSortName.Font = new Font(toolStrip.Font,
            CurrentSort.Field == SortField.Name ? FontStyle.Bold : FontStyle.Regular);
        btnSortDate.Font = new Font(toolStrip.Font,
            CurrentSort.Field == SortField.DateCreated ? FontStyle.Bold : FontStyle.Regular);
    }

    private void ResizeNameColumn()
    {
        if (listView.Columns.Count == 0) return;
        if (_savedNameColumnWidth > 0) return;
        var used = listView.Columns[1].Width + listView.Columns[2].Width;
        var available = listView.ClientSize.Width - used - SystemInformation.VerticalScrollBarWidth;
        if (available > 50) listView.Columns[0].Width = available;
    }

    private void OnColumnWidthChanged(object? sender, ColumnWidthChangedEventArgs e)
    {
        if (e.ColumnIndex == 0)
        {
            _savedNameColumnWidth = listView.Columns[0].Width;
            NameColumnWidthChanged?.Invoke(this, _savedNameColumnWidth);
        }
    }

    private void OnSortNameClick(object? sender, EventArgs e)
    {
        CurrentSort = CurrentSort.WithField(SortField.Name);
        PopulateList();
        UpdateSortButtons();
        SortChanged?.Invoke(this, CurrentSort);
    }

    private void OnSortDateClick(object? sender, EventArgs e)
    {
        CurrentSort = CurrentSort.WithField(SortField.DateCreated);
        PopulateList();
        UpdateSortButtons();
        SortChanged?.Invoke(this, CurrentSort);
    }

    private void OnListViewClick(object? sender, EventArgs e)
    {
        if (SelectedFile is { } file)
            FileSelected?.Invoke(this, file);
    }

    private void OnListViewDoubleClick(object? sender, EventArgs e)
    {
        if (SelectedFile is { } file)
            FileDoubleClicked?.Invoke(this, file);
    }

    public event EventHandler<SortOptions>? SortChanged;
    public event EventHandler<int>? NameColumnWidthChanged;

    public void SetNameColumnWidth(int width)
    {
        _savedNameColumnWidth = width;
        if (listView.Columns.Count > 0 && width > 0)
            listView.Columns[0].Width = width;
    }
}
