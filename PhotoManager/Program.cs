using PhotoManager.Settings;

namespace PhotoManager;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var settingsService = new SettingsService();
        var settings = settingsService.Load();

        var form = new MainForm(settings);
        form.FormClosed += (_, _) => settingsService.Save(settings);

        Application.Run(form);
    }
}
