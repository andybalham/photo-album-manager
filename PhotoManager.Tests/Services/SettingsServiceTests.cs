using PhotoManager.Models;
using PhotoManager.Settings;

namespace PhotoManager.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly string _tempPath;
    private readonly SettingsService _sut;

    public SettingsServiceTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _sut = new SettingsService(_tempPath);
    }

    [Fact]
    public void Load_ReturnsDefaults_WhenFileDoesNotExist()
    {
        var settings = _sut.Load();

        Assert.Equal(string.Empty, settings.SourceFolderPath);
        Assert.Equal(string.Empty, settings.TargetFolderPath);
        Assert.Equal(SortField.Name, settings.SortField);
        Assert.Equal(SortDirection.Ascending, settings.SortDirection);
        Assert.Equal(280, settings.SplitterPosition);
    }

    [Fact]
    public void SaveThenLoad_RoundTripsAllFields()
    {
        var original = new AppSettings
        {
            SourceFolderPath = @"C:\Source",
            TargetFolderPath = @"C:\Target",
            SortField = SortField.DateCreated,
            SortDirection = SortDirection.Descending,
            SplitterPosition = 400
        };

        _sut.Save(original);
        var loaded = _sut.Load();

        Assert.Equal(original.SourceFolderPath, loaded.SourceFolderPath);
        Assert.Equal(original.TargetFolderPath, loaded.TargetFolderPath);
        Assert.Equal(original.SortField, loaded.SortField);
        Assert.Equal(original.SortDirection, loaded.SortDirection);
        Assert.Equal(original.SplitterPosition, loaded.SplitterPosition);
    }

    [Fact]
    public void Load_ReturnsDefaults_WhenFileIsCorrupt()
    {
        Directory.CreateDirectory(_tempPath);
        File.WriteAllText(Path.Combine(_tempPath, "appsettings.json"), "{ invalid json {{{{");

        var settings = _sut.Load();

        Assert.Equal(string.Empty, settings.SourceFolderPath);
        Assert.Equal(280, settings.SplitterPosition);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempPath))
            Directory.Delete(_tempPath, recursive: true);
    }
}
