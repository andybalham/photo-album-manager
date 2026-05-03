using PhotoManager.Models;
using PhotoManager.Services;

namespace PhotoManager.Controls;

public enum PreviewContext { Source, Target, Removed }

public partial class PreviewPanel : UserControl
{
    private readonly ImageLoadService _imageService;
    private readonly FileOperationService _fileOpService;

    private readonly ToolTip _toolTip = new();
    private List<ImageFile> _files = [];
    private int _index = -1;
    private PreviewContext _context;
    private string _sourceRoot = string.Empty;
    private string _targetRoot = string.Empty;
    private SortOptions _sort = SortOptions.Default;
    private Bitmap? _currentBitmap;
    private bool _operationInProgress;

    public event EventHandler<ImageFile>? FileActioned;
    public event EventHandler<SortOptions>? SortChanged;
    public event EventHandler<string>? StatusMessage;

    public PreviewContext Context => _context;
    public bool CanAction => btnAction.Enabled;
    public void NavigatePrev() => OnPrevClick(null, EventArgs.Empty);
    public void NavigateNext() => OnNextClick(null, EventArgs.Empty);
    public void TriggerAction() => _ = OnActionClickAsync();

    public PreviewPanel(ImageLoadService imageService, FileOperationService fileOpService)
    {
        _imageService = imageService;
        _fileOpService = fileOpService;
        InitializeComponent();
    }

    public async Task LoadFolderAsync(
        IReadOnlyList<ImageFile> files,
        int selectedIndex,
        SortOptions sort,
        PreviewContext context,
        string sourceRoot,
        string targetRoot)
    {
        _files = [.. files];
        _sort = sort;
        _context = context;
        _sourceRoot = sourceRoot;
        _targetRoot = targetRoot;
        _index = files.Count > 0 ? Math.Clamp(selectedIndex, 0, files.Count - 1) : -1;

        UpdateSortButton();
        UpdateActionButton();
        await ShowCurrentAsync();
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private async void OnPrevClick(object? sender, EventArgs e)
    {
        if (_files.Count == 0) return;
        _index = (_index - 1 + _files.Count) % _files.Count;
        await ShowCurrentAsync();
    }

    private async void OnNextClick(object? sender, EventArgs e)
    {
        if (_files.Count == 0) return;
        _index = (_index + 1) % _files.Count;
        await ShowCurrentAsync();
    }

    // ── Sort ──────────────────────────────────────────────────────────────────

    private void OnSortClick(object? sender, EventArgs e)
    {
        _sort = (_sort.Field, _sort.Direction) switch
        {
            (SortField.Name, SortDirection.Ascending)        => new(SortField.Name, SortDirection.Descending),
            (SortField.Name, SortDirection.Descending)       => new(SortField.DateCreated, SortDirection.Ascending),
            (SortField.DateCreated, SortDirection.Ascending) => new(SortField.DateCreated, SortDirection.Descending),
            _                                                => new(SortField.Name, SortDirection.Ascending)
        };

        var current = _index >= 0 ? _files[_index] : null;
        ApplySort();
        _index = current != null ? Math.Max(0, _files.IndexOf(current)) : 0;

        UpdateSortButton();
        UpdatePosition();
        SortChanged?.Invoke(this, _sort);
    }

    private void ApplySort()
    {
        IEnumerable<ImageFile> sorted = _sort.Field == SortField.DateCreated
            ? _files.OrderBy(f => f.DateCreated)
            : _files.OrderBy(f => f.FileName, StringComparer.OrdinalIgnoreCase);
        if (_sort.Direction == SortDirection.Descending)
            sorted = sorted.Reverse();
        _files = [.. sorted];
    }

    private void UpdateSortButton() =>
        btnSort.Text = (_sort.Field, _sort.Direction) switch
        {
            (SortField.Name, SortDirection.Ascending)        => "Name ↑",
            (SortField.Name, SortDirection.Descending)       => "Name ↓",
            (SortField.DateCreated, SortDirection.Ascending) => "Date ↑",
            _                                                => "Date ↓"
        };

    // ── Action ────────────────────────────────────────────────────────────────

    private void UpdateActionButton()
    {
        bool noTarget = _context == PreviewContext.Source && string.IsNullOrEmpty(_targetRoot);
        btnAction.Text = _context switch
        {
            PreviewContext.Source  => "Copy to Target",
            PreviewContext.Target  => "Remove",
            PreviewContext.Removed => "Undo",
            _                      => string.Empty
        };
        btnAction.Enabled = _index >= 0 && !_operationInProgress && !noTarget;
        _toolTip.SetToolTip(btnAction, noTarget ? "Select a target folder first" : string.Empty);
    }

    private void OnActionClick(object? sender, EventArgs e) => _ = OnActionClickAsync();

    private async Task OnActionClickAsync()
    {
        if (_index < 0 || _index >= _files.Count || _operationInProgress) return;

        var file = _files[_index];
        _operationInProgress = true;
        btnAction.Enabled = false;

        try
        {
            bool acted;
            switch (_context)
            {
                case PreviewContext.Source:
                    acted = await _fileOpService.CopyToTargetAsync(file, _sourceRoot, _targetRoot);
                    break;
                case PreviewContext.Target:
                    await _fileOpService.MoveToRemovedAsync(file, _targetRoot);
                    acted = true;
                    break;
                case PreviewContext.Removed:
                    acted = await _fileOpService.UndoRemoveAsync(file, _targetRoot);
                    break;
                default:
                    acted = false;
                    break;
            }

            if (acted)
            {
                _files.RemoveAt(_index);
                if (_files.Count == 0)
                    _index = -1;
                else
                    _index = Math.Min(_index, _files.Count - 1);

                FileActioned?.Invoke(this, file);
                UpdateActionButton();
                await ShowCurrentAsync();
            }
            else
            {
                var skipMsg = _context == PreviewContext.Source
                    ? "File already exists in target — skipped."
                    : "File already exists in target — skipped.";
                StatusMessage?.Invoke(this, skipMsg);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(ex.Message, "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            _operationInProgress = false;
            btnAction.Enabled = _index >= 0;
        }
    }

    // ── Image display ─────────────────────────────────────────────────────────

    private async Task ShowCurrentAsync()
    {
        if (_index < 0 || _index >= _files.Count)
        {
            ShowBlank();
            return;
        }

        var file = _files[_index];
        lblFileInfo.Text = $"{file.FileName}  —  {file.FormattedDate}";
        UpdatePosition();
        btnPrev.Enabled = _files.Count > 1;
        btnNext.Enabled = _files.Count > 1;

        picBox.Visible = false;
        lblError.Text = "Loading…";
        lblError.Visible = true;

        var displaySize = pnlImage.ClientSize;
        if (displaySize.Width <= 0) displaySize = new Size(800, 600);

        var (bitmap, error) = await _imageService.LoadAsync(file.FullPath, displaySize);

        _currentBitmap?.Dispose();
        _currentBitmap = bitmap;

        if (bitmap != null)
        {
            picBox.Image = bitmap;
            picBox.Visible = true;
            lblError.Visible = false;
        }
        else
        {
            lblError.Text = error ?? "Cannot preview this image";
            picBox.Visible = false;
            lblError.Visible = true;
        }
    }

    private void ShowBlank()
    {
        lblFileInfo.Text = string.Empty;
        lblPosition.Text = string.Empty;
        picBox.Image = null;
        picBox.Visible = false;
        lblError.Visible = false;
        btnPrev.Enabled = false;
        btnNext.Enabled = false;
        btnAction.Enabled = false;
    }

    private void UpdatePosition() =>
        lblPosition.Text = _files.Count > 0 ? $"{_index + 1} / {_files.Count}" : string.Empty;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _currentBitmap?.Dispose();
            _toolTip.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }
}
