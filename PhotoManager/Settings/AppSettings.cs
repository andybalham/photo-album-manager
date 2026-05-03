using PhotoManager.Models;

namespace PhotoManager.Settings;

public class AppSettings
{
    public string SourceFolderPath { get; set; } = string.Empty;
    public string TargetFolderPath { get; set; } = string.Empty;
    public SortField SortField { get; set; } = SortField.Name;
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
    public int SplitterPosition { get; set; } = 280;
}
