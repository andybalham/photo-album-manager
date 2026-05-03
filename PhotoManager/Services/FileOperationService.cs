using PhotoManager.Models;

namespace PhotoManager.Services;

public class FileOperationService
{
    public Task<bool> CopyToTargetAsync(ImageFile file, string sourceRoot, string targetRoot) =>
        Task.Run(() =>
        {
            var destPath = Path.Combine(targetRoot, file.RelativePath);
            if (File.Exists(destPath))
                return false;

            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
            File.Copy(file.FullPath, destPath);
            return true;
        });

    public Task MoveToRemovedAsync(ImageFile file, string targetRoot) =>
        Task.Run(() =>
        {
            var destPath = Path.Combine(targetRoot, "_removed", file.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
            File.Move(file.FullPath, destPath);
        });

    public Task<bool> UndoRemoveAsync(ImageFile file, string targetRoot) =>
        Task.Run(() =>
        {
            var destPath = Path.Combine(targetRoot, file.RelativePath);
            if (File.Exists(destPath))
                return false;

            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
            File.Move(file.FullPath, destPath);
            return true;
        });
}
