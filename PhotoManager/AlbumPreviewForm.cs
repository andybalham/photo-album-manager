using PhotoManager.Models;
using PhotoManager.Services;

namespace PhotoManager;

public class AlbumPreviewForm : Form
{
    private const int ThumbnailSize = 160;
    private const int CellPad = 6;
    private const int LabelHeight = 38;

    private readonly FolderScanService _scanService;
    private readonly FileOperationService _fileOpService;
    private readonly ImageLoadService _imageService;
    private readonly string _targetRoot;

    private List<ImageFile> _files = [];
    private int _selectedIndex = -1;
    private Bitmap? _previewBitmap;
    private bool _operationInProgress;
    private CancellationTokenSource _loadCts = new();

    private readonly FlowLayoutPanel _flowPanel;
    private readonly PictureBox _picPreview;
    private readonly Label _lblPreviewInfo;
    private readonly Label _lblPreviewError;
    private readonly Panel _pnlPreviewImage;
    private readonly Button _btnRemove;

    public event EventHandler<ImageFile>? FileRemoved;

    public AlbumPreviewForm(
        FolderScanService scanService,
        FileOperationService fileOpService,
        ImageLoadService imageService,
        string targetRoot)
    {
        _scanService = scanService;
        _fileOpService = fileOpService;
        _imageService = imageService;
        _targetRoot = targetRoot;

        Text = $"Album — {targetRoot}";
        ClientSize = new Size(1100, 700);
        MinimumSize = new Size(700, 500);
        StartPosition = FormStartPosition.CenterParent;
        KeyPreview = true;

        _flowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            BackColor = SystemColors.AppWorkspace,
            Padding = new Padding(4)
        };

        _lblPreviewInfo = new Label
        {
            Dock = DockStyle.Top,
            Height = 24,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0)
        };

        _lblPreviewError = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = SystemColors.GrayText,
            Text = "Loading…",
            Visible = true
        };

        _picPreview = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Black,
            Visible = false
        };

        _pnlPreviewImage = new Panel { Dock = DockStyle.Fill };
        _pnlPreviewImage.Controls.Add(_picPreview);
        _pnlPreviewImage.Controls.Add(_lblPreviewError);

        _btnRemove = new Button
        {
            Text = "Remove (Del)",
            Dock = DockStyle.Bottom,
            Height = 32,
            Enabled = false
        };
        _btnRemove.Click += (_, _) => _ = RemoveCurrentAsync();

        var rightPanel = new Panel { Dock = DockStyle.Fill };
        rightPanel.Controls.Add(_pnlPreviewImage);
        rightPanel.Controls.Add(_lblPreviewInfo);
        rightPanel.Controls.Add(_btnRemove);

        var splitter = new SplitContainer { Dock = DockStyle.Fill };
        splitter.Panel1.Controls.Add(_flowPanel);
        splitter.Panel2.Controls.Add(rightPanel);

        Controls.Add(splitter);
        KeyDown += OnKeyDown;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        var sc = Controls.OfType<SplitContainer>().First();
        sc.Panel1MinSize = 200;
        sc.Panel2MinSize = 300;
        sc.SplitterDistance = Math.Clamp(420, sc.Panel1MinSize, sc.Width - sc.Panel2MinSize);
        await LoadAlbumAsync();
    }

    // ── Load ──────────────────────────────────────────────────────────────────

    private async Task LoadAlbumAsync()
    {
        _loadCts.Cancel();
        _loadCts = new CancellationTokenSource();
        var ct = _loadCts.Token;

        _flowPanel.Controls.Clear();
        _files = [];
        _selectedIndex = -1;
        ShowPreviewPlaceholder("Loading…");

        var files = await _scanService.GetFilesInFolderAsync(_targetRoot, _targetRoot);
        if (ct.IsCancellationRequested) return;

        _files = [.. files];

        if (_files.Count == 0)
        {
            ShowPreviewPlaceholder("No images in album");
            return;
        }

        foreach (var file in _files)
            _flowPanel.Controls.Add(BuildThumbnailCell(file));

        SelectIndex(0);
        _ = LoadThumbnailsAsync(ct);
    }

    private async Task LoadThumbnailsAsync(CancellationToken ct)
    {
        for (int i = 0; i < _files.Count; i++)
        {
            if (ct.IsCancellationRequested) return;

            var (bmp, _) = await _imageService.LoadAsync(
                _files[i].FullPath, new Size(ThumbnailSize, ThumbnailSize));

            if (ct.IsCancellationRequested) { bmp?.Dispose(); return; }
            if (i >= _flowPanel.Controls.Count) continue;

            var cell = _flowPanel.Controls[i];
            var pic = cell.Controls.OfType<PictureBox>().FirstOrDefault();
            if (pic != null)
            {
                var old = pic.Image;
                pic.Image = bmp;
                old?.Dispose();
            }
        }
    }

    // ── Thumbnail cells ───────────────────────────────────────────────────────

    private Panel BuildThumbnailCell(ImageFile file)
    {
        int cellWidth = ThumbnailSize + CellPad * 2;
        int cellHeight = ThumbnailSize + LabelHeight + CellPad * 2;

        var cell = new Panel
        {
            Width = cellWidth,
            Height = cellHeight,
            Margin = new Padding(4),
            Cursor = Cursors.Hand,
            BackColor = SystemColors.AppWorkspace
        };

        var pic = new PictureBox
        {
            Left = CellPad,
            Top = CellPad,
            Width = ThumbnailSize,
            Height = ThumbnailSize,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.DimGray
        };

        var lbl = new Label
        {
            Left = 0,
            Top = ThumbnailSize + CellPad * 2,
            Width = cellWidth,
            Height = LabelHeight,
            Text = file.FileName,
            TextAlign = ContentAlignment.TopCenter,
            AutoEllipsis = true,
            ForeColor = SystemColors.ControlText
        };

        cell.Controls.Add(pic);
        cell.Controls.Add(lbl);

        cell.Click += OnCellClick;
        pic.Click += OnCellClick;
        lbl.Click += OnCellClick;

        return cell;
    }

    private void OnCellClick(object? sender, EventArgs e)
    {
        var ctrl = sender as Control;
        while (ctrl != null && ctrl.Parent != _flowPanel)
            ctrl = ctrl.Parent;
        if (ctrl == null) return;
        int index = _flowPanel.Controls.IndexOf(ctrl);
        if (index >= 0) SelectIndex(index);
    }

    private static void SetCellSelected(Control cell, bool selected)
    {
        cell.BackColor = selected ? SystemColors.Highlight : SystemColors.AppWorkspace;
        var lbl = cell.Controls.OfType<Label>().FirstOrDefault();
        if (lbl != null)
            lbl.ForeColor = selected ? SystemColors.HighlightText : SystemColors.ControlText;
    }

    // ── Selection & preview ───────────────────────────────────────────────────

    private void SelectIndex(int index)
    {
        if (_files.Count == 0) return;
        index = Math.Clamp(index, 0, _files.Count - 1);

        if (_selectedIndex >= 0 && _selectedIndex < _flowPanel.Controls.Count)
            SetCellSelected(_flowPanel.Controls[_selectedIndex], false);

        _selectedIndex = index;

        if (_selectedIndex < _flowPanel.Controls.Count)
        {
            var cell = _flowPanel.Controls[_selectedIndex];
            SetCellSelected(cell, true);
            _flowPanel.ScrollControlIntoView(cell);
        }

        _btnRemove.Enabled = !_operationInProgress;
        _ = LoadPreviewAsync(_files[_selectedIndex]);
    }

    private async Task LoadPreviewAsync(ImageFile file)
    {
        _lblPreviewInfo.Text = $"{file.FileName}  —  {file.FormattedDate}";
        _picPreview.Visible = false;
        _lblPreviewError.Text = "Loading…";
        _lblPreviewError.Visible = true;

        var displaySize = _pnlPreviewImage.ClientSize;
        if (displaySize.Width <= 0) displaySize = new Size(600, 600);

        var (bmp, error) = await _imageService.LoadAsync(file.FullPath, displaySize);

        if (_selectedIndex < 0 || _selectedIndex >= _files.Count || _files[_selectedIndex] != file)
        {
            bmp?.Dispose();
            return;
        }

        _previewBitmap?.Dispose();
        _previewBitmap = bmp;

        if (bmp != null)
        {
            _picPreview.Image = bmp;
            _picPreview.Visible = true;
            _lblPreviewError.Visible = false;
        }
        else
        {
            _lblPreviewError.Text = error ?? "Cannot preview this image";
            _lblPreviewError.Visible = true;
            _picPreview.Visible = false;
        }
    }

    private void ShowPreviewPlaceholder(string message)
    {
        _lblPreviewInfo.Text = string.Empty;
        _picPreview.Image = null;
        _picPreview.Visible = false;
        _lblPreviewError.Text = message;
        _lblPreviewError.Visible = true;
        _btnRemove.Enabled = false;
    }

    // ── Keyboard ──────────────────────────────────────────────────────────────

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Left:
            case Keys.Up:
                if (_selectedIndex > 0) SelectIndex(_selectedIndex - 1);
                e.Handled = true;
                break;
            case Keys.Right:
            case Keys.Down:
                if (_selectedIndex < _files.Count - 1) SelectIndex(_selectedIndex + 1);
                e.Handled = true;
                break;
            case Keys.Delete:
                if (_btnRemove.Enabled) _ = RemoveCurrentAsync();
                e.Handled = true;
                break;
        }
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    private async Task RemoveCurrentAsync()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _files.Count || _operationInProgress) return;

        var file = _files[_selectedIndex];
        _operationInProgress = true;
        _btnRemove.Enabled = false;

        try
        {
            await _fileOpService.MoveToRemovedAsync(file, _targetRoot);

            if (_selectedIndex < _flowPanel.Controls.Count)
            {
                var cell = _flowPanel.Controls[_selectedIndex];
                var pic = cell.Controls.OfType<PictureBox>().FirstOrDefault();
                if (pic?.Image != null)
                {
                    var img = pic.Image;
                    pic.Image = null;
                    img.Dispose();
                }
                _flowPanel.Controls.RemoveAt(_selectedIndex);
            }

            _files.RemoveAt(_selectedIndex);
            FileRemoved?.Invoke(this, file);

            if (_files.Count == 0)
            {
                _selectedIndex = -1;
                ShowPreviewPlaceholder("No images in album");
            }
            else
            {
                var next = Math.Min(_selectedIndex, _files.Count - 1);
                _selectedIndex = -1;
                SelectIndex(next);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(ex.Message, "Remove Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            _operationInProgress = false;
            if (_selectedIndex >= 0) _btnRemove.Enabled = true;
        }
    }

    // ── Dispose ───────────────────────────────────────────────────────────────

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _loadCts.Cancel();
            _loadCts.Dispose();
            _previewBitmap?.Dispose();
            foreach (Control cell in _flowPanel.Controls)
                foreach (var pic in cell.Controls.OfType<PictureBox>())
                    pic.Image?.Dispose();
        }
        base.Dispose(disposing);
    }
}
