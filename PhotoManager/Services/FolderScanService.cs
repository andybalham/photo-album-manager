using PhotoManager.Helpers;
using PhotoManager.Models;

namespace PhotoManager.Services;

public class FolderScanService
{
    public Task<IReadOnlyList<ImageFile>> GetFilesInFolderAsync(string folderPath, string rootPath) =>
        Task.Run<IReadOnlyList<ImageFile>>(() =>
        {
            if (!Directory.Exists(folderPath))
                return [];

            return Directory
                .EnumerateFiles(folderPath)
                .Where(ImageFormatHelper.IsImageFile)
                .Select(path =>
                {
                    var info = new FileInfo(path);
                    var relative = Path.GetRelativePath(rootPath, path);
                    return new ImageFile(path, relative, info.Name, info.CreationTime, info.Length);
                })
                .ToList();
        });

    public Task<IReadOnlyList<string>> GetImageSubfoldersAsync(string folderPath) =>
        Task.Run<IReadOnlyList<string>>(() =>
        {
            if (!Directory.Exists(folderPath))
                return [];

            return Directory
                .EnumerateDirectories(folderPath)
                .Where(d => !IsRemovedFolder(d) && ContainsImages(d))
                .ToList();
        });

    public Task<int> GetImageCountAsync(string folderPath) =>
        Task.Run(() =>
        {
            if (!Directory.Exists(folderPath))
                return 0;

            return Directory
                .EnumerateFiles(folderPath)
                .Count(ImageFormatHelper.IsImageFile);
        });

    private static bool IsRemovedFolder(string path) =>
        string.Equals(Path.GetFileName(path), "_removed", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsImages(string folderPath)
    {
        try
        {
            if (Directory.EnumerateFiles(folderPath).Any(ImageFormatHelper.IsImageFile))
                return true;

            return Directory
                .EnumerateDirectories(folderPath)
                .Where(d => !IsRemovedFolder(d))
                .Any(ContainsImages);
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
