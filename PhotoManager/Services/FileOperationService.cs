using PhotoManager.Helpers;
using PhotoManager.Models;

namespace PhotoManager.Services;

public enum CopyResult { Success, ConflictInTarget, ConflictInRemoved }

public class FileOperationService
{
    public Task<CopyResult> CopyToTargetAsync(ImageFile file, string targetRoot) =>
        Task.Run(() =>
        {
            var destPath = Path.Combine(targetRoot, file.FileName);
            if (File.Exists(destPath))
                return CopyResult.ConflictInTarget;

            var removedPath = Path.Combine(targetRoot, "_removed", file.FileName);
            if (File.Exists(removedPath))
            {
                var removedInfo = new FileInfo(removedPath);
                if (ImageFormatHelper.GetFileDate(removedPath) == ImageFormatHelper.GetFileDate(file.FullPath)
                    && removedInfo.Length == file.FileSizeBytes)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                    File.Move(removedPath, destPath);
                    return CopyResult.Success;
                }
                return CopyResult.ConflictInRemoved;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
            File.Copy(file.FullPath, destPath);
            return CopyResult.Success;
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
