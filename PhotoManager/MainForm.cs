using PhotoManager.Settings;

namespace PhotoManager;

public partial class MainForm : Form
{
    private readonly AppSettings _settings;

    public MainForm(AppSettings settings)
    {
        _settings = settings;
        InitializeComponent();

        btnSelectSource.Click += OnSelectSource;
        btnSelectTarget.Click += OnSelectTarget;
        splitContainer.SplitterMoved += OnSplitterMoved;
        Load += OnLoad;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        splitContainer.SplitterDistance = _settings.SplitterPosition;

        if (!string.IsNullOrEmpty(_settings.SourceFolderPath))
            lblSourcePath.Text = _settings.SourceFolderPath;

        if (!string.IsNullOrEmpty(_settings.TargetFolderPath))
            lblTargetPath.Text = _settings.TargetFolderPath;
    }

    private void OnSelectSource(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog();
        if (dlg.ShowDialog() != DialogResult.OK) return;

        _settings.SourceFolderPath = dlg.SelectedPath;
        lblSourcePath.Text = dlg.SelectedPath;
    }

    private void OnSelectTarget(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog();
        if (dlg.ShowDialog() != DialogResult.OK) return;

        _settings.TargetFolderPath = dlg.SelectedPath;
        lblTargetPath.Text = dlg.SelectedPath;
    }

    private void OnSplitterMoved(object? sender, SplitterEventArgs e)
    {
        _settings.SplitterPosition = splitContainer.SplitterDistance;
    }
}
