using PhotoManager.Settings;

namespace PhotoManager;

public partial class MainForm : Form
{
    private readonly AppSettings _settings;

    public MainForm(AppSettings settings)
    {
        _settings = settings;
        InitializeComponent();
    }
}
