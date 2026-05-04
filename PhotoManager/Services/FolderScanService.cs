using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using PhotoManager.Helpers;
using PhotoManager.Models;

namespace PhotoManager.Services;

public class FolderScanService
{
    public Task<IReadOnlyList<ImageFile>> GetFilesInFolderAsync(string folderPath, string rootPath,
        IReadOnlyList<string>? excludeRoots = null) =>
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
                    var date = TryGetExifDate(path) ?? info.LastWriteTime;
                    return new ImageFile(path, relative, info.Name, date, info.Length);
                })
                .Where(f => excludeRoots == null || !excludeRoots.Any(
                    root => File.Exists(Path.Combine(root, f.RelativePath))))
                .ToList();
        });

    public async IAsyncEnumerable<ImageFile> StreamFilesInFolderAsync(string folderPath, string rootPath,
        IReadOnlyList<string>? excludeRoots = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var channel = Channel.CreateUnbounded<ImageFile>(new UnboundedChannelOptions { SingleReader = true });

        var producer = Task.Run(() =>
        {
            try
            {
                if (!Directory.Exists(folderPath)) return;
                foreach (var path in Directory.EnumerateFiles(folderPath))
                {
                    if (ct.IsCancellationRequested) break;
                    if (!ImageFormatHelper.IsImageFile(path)) continue;
                    var info = new FileInfo(path);
                    var relative = Path.GetRelativePath(rootPath, path);
                    var date = TryGetExifDate(path) ?? info.LastWriteTime;
                    var file = new ImageFile(path, relative, info.Name, date, info.Length);
                    if (excludeRoots == null || !excludeRoots.Any(
                            root => File.Exists(Path.Combine(root, file.RelativePath))))
                        channel.Writer.TryWrite(file);
                }
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, ct);

        await foreach (var file in channel.Reader.ReadAllAsync(ct))
            yield return file;

        await producer;
    }

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

    // EXIF tag 0x9003 = DateTimeOriginal, 0x0132 = DateTime; format "yyyy:MM:dd HH:mm:ss"
    private static DateTime? TryGetExifDate(string path)
    {
        try
        {
            using var img = Image.FromFile(path);
            foreach (var tagId in new[] { 0x9003, 0x0132 })
            {
                try
                {
                    var prop = img.GetPropertyItem(tagId);
                    if (prop?.Value == null) continue;
                    var raw = Encoding.ASCII.GetString(prop.Value).TrimEnd('\0');
                    if (DateTime.TryParseExact(raw, "yyyy:MM:dd HH:mm:ss",
                            null, System.Globalization.DateTimeStyles.None, out var dt))
                        return dt;
                }
                catch (ArgumentException) { }
            }
        }
        catch { }
        return null;
    }

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
