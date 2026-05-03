using PhotoManager.Helpers;
using PhotoManager.Settings;

namespace PhotoManager;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var splash = new SplashForm();
        splash.Show();
        splash.Refresh();

        var settingsService = new SettingsService();
        var settings = settingsService.Load();

        var icon = IconHelper.GetAppIcon();

        var splashStart = DateTime.UtcNow;

        var form = new MainForm(settings, icon);
        form.FormClosed += (_, _) => settingsService.Save(settings);
        form.Shown += (_, _) =>
        {
            var elapsed = (DateTime.UtcNow - splashStart).TotalMilliseconds;
            var remaining = (int)(1000 - elapsed);
            if (remaining > 0)
                Task.Delay(remaining).ContinueWith(_ => splash.Invoke(splash.Close));
            else
                splash.Close();
        };

        Application.Run(form);
    }
}
