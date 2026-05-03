using System.Text.Json;

namespace PhotoManager.Settings;

public class SettingsService
{
    private readonly string _settingsPath;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public SettingsService(string? settingsDirectory = null)
    {
        var dir = settingsDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PhotoManager");
        _settingsPath = Path.Combine(dir, "appsettings.json");
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return new AppSettings();

            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_settingsPath, json);
    }
}
