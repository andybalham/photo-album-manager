namespace PhotoManager.Models;

public record ImageFile(
    string FullPath,
    string RelativePath,
    string FileName,
    DateTime DateCreated,
    long FileSizeBytes)
{
    public string FormattedSize => FileSizeBytes >= 1_048_576
        ? $"{FileSizeBytes / 1_048_576.0:F1} MB"
        : $"{FileSizeBytes / 1024.0:F1} KB";

    public string FormattedDate => DateCreated.ToString("dd MMM yyyy HH:mm");
}
