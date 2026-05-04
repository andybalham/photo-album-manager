using PhotoManager.Models;

namespace PhotoManager.Settings;

public class AppSettings
{
    public string SourceFolderPath { get; set; } = string.Empty;
    public string TargetFolderPath { get; set; } = string.Empty;
    public SortField SortField { get; set; } = SortField.Name;
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
    public int SplitterPosition { get; set; } = 280;
    public int NameColumnWidth { get; set; } = 0;
    public int WindowWidth { get; set; } = 1100;
    public int WindowHeight { get; set; } = 700;
    public int WindowLeft { get; set; } = -1;
    public int WindowTop { get; set; } = -1;
    public string WindowState { get; set; } = "Normal";
    public int AlbumWindowWidth { get; set; } = 1100;
    public int AlbumWindowHeight { get; set; } = 700;
    public int AlbumWindowLeft { get; set; } = -1;
    public int AlbumWindowTop { get; set; } = -1;
    public string AlbumWindowState { get; set; } = "Normal";
    public int AlbumSplitterPosition { get; set; } = 420;
}
