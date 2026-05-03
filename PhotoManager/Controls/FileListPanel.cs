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

    public async Task LoadFolderAsync(string folderPath, string rootPath, SortOptions sort)
    {
        _currentFolder = folderPath;
        _currentRoot = rootPath;
        CurrentSort = sort;

        _files = [.. (await _scanService.GetFilesInFolderAsync(folderPath, rootPath))];
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

    private void PopulateList()
    {
        IEnumerable<ImageFile> sorted = CurrentSort.Field == SortField.DateCreated
            ? _files.OrderBy(f => f.DateCreated)
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
