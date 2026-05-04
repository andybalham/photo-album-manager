using PhotoManager.Models;
using PhotoManager.Services;
using PhotoManager.Tests.Helpers;

namespace PhotoManager.Tests.Services;

public class FileOperationServiceTests : IDisposable
{
    private readonly TempFolderFixture _folder = new();
    private readonly FileOperationService _sut = new();

    private ImageFile MakeFile(string relativePath)
    {
        var full = _folder.CreateImageFile(relativePath);
        var info = new FileInfo(full);
        return new ImageFile(full, relativePath, info.Name, info.LastWriteTime, info.Length);
    }

    // CopyToTargetAsync

    [Fact]
    public async Task CopyToTarget_CreatesFileAtTargetRoot()
    {
        var source = _folder.CreateFolder("source");
        var target = _folder.CreateFolder("target");
        var file = MakeFile(@"source\2024\photo.jpg");

        var result = await _sut.CopyToTargetAsync(file, target);

        Assert.Equal(CopyResult.Success, result);
        Assert.True(File.Exists(Path.Combine(target, "photo.jpg")));
        Assert.True(File.Exists(file.FullPath)); // source untouched
    }

    [Fact]
    public async Task CopyToTarget_ReturnsConflictInTarget_WhenDestinationExists()
    {
        var source = _folder.CreateFolder("source2");
        var target = _folder.CreateFolder("target2");
        var file = MakeFile(@"source2\photo.jpg");

        File.WriteAllText(Path.Combine(target, "photo.jpg"), "existing");

        var result = await _sut.CopyToTargetAsync(file, target);

        Assert.Equal(CopyResult.ConflictInTarget, result);
        Assert.Equal("existing", File.ReadAllText(Path.Combine(target, "photo.jpg")));
        Assert.True(File.Exists(file.FullPath));
    }

    [Fact]
    public async Task CopyToTarget_UndoesRemoved_WhenNameDateSizeMatch()
    {
        var source = _folder.CreateFolder("source3");
        var target = _folder.CreateFolder("target3");
        var file = MakeFile(@"source3\photo.jpg");

        // Place matching file in _removed
        var removedPath = Path.Combine(target, "_removed", "photo.jpg");
        Directory.CreateDirectory(Path.GetDirectoryName(removedPath)!);
        File.Copy(file.FullPath, removedPath);
        var ts = new DateTime(2024, 6, 1, 12, 0, 0);
        foreach (var p in new[] { file.FullPath, removedPath })
        {
            File.SetCreationTime(p, ts);
            File.SetLastWriteTime(p, ts);
        }

        var result = await _sut.CopyToTargetAsync(file, target);

        Assert.Equal(CopyResult.Success, result);
        Assert.True(File.Exists(Path.Combine(target, "photo.jpg")));
        Assert.False(File.Exists(removedPath)); // moved out of _removed
    }

    [Fact]
    public async Task CopyToTarget_ReturnsConflictInRemoved_WhenMetadataDiffers()
    {
        var source = _folder.CreateFolder("source4");
        var target = _folder.CreateFolder("target4");
        var file = MakeFile(@"source4\photo.jpg");

        // Place file with different content/date in _removed
        var removedPath = Path.Combine(target, "_removed", "photo.jpg");
        Directory.CreateDirectory(Path.GetDirectoryName(removedPath)!);
        File.WriteAllText(removedPath, "different content");
        File.SetLastWriteTime(removedPath, DateTime.Now.AddDays(-1));

        var result = await _sut.CopyToTargetAsync(file, target);

        Assert.Equal(CopyResult.ConflictInRemoved, result);
        Assert.False(File.Exists(Path.Combine(target, "photo.jpg")));
        Assert.True(File.Exists(removedPath)); // _removed untouched
    }

    // MoveToRemovedAsync

    [Fact]
    public async Task MoveToRemoved_MovesFileToRemovedSubfolder()
    {
        var target = _folder.CreateFolder("target5");
        var file = MakeFile(@"target5\2024\photo.jpg");
        var relative = Path.GetRelativePath(target, file.FullPath);
        var imageFile = file with { RelativePath = relative };

        await _sut.MoveToRemovedAsync(imageFile, target);

        var expectedDest = Path.Combine(target, "_removed", relative);
        Assert.True(File.Exists(expectedDest));
        Assert.False(File.Exists(imageFile.FullPath));
    }

    // UndoRemoveAsync

    [Fact]
    public async Task UndoRemove_MovesFileBackToTarget()
    {
        var target = _folder.CreateFolder("target6");
        var relative = Path.Combine("2024", "photo.jpg");
        var removedPath = _folder.CreateImageFile(Path.Combine("target6", "_removed", relative));
        var info = new FileInfo(removedPath);
        var imageFile = new ImageFile(removedPath, relative, info.Name, info.LastWriteTime, info.Length);

        var result = await _sut.UndoRemoveAsync(imageFile, target);

        Assert.True(result);
        Assert.True(File.Exists(Path.Combine(target, relative)));
        Assert.False(File.Exists(removedPath));
    }

    [Fact]
    public async Task UndoRemove_ReturnsFalse_WhenTargetAlreadyExists()
    {
        var target = _folder.CreateFolder("target7");
        var relative = Path.Combine("2024", "photo.jpg");

        var removedPath = _folder.CreateImageFile(Path.Combine("target7", "_removed", relative));
        var info = new FileInfo(removedPath);
        var imageFile = new ImageFile(removedPath, relative, info.Name, info.LastWriteTime, info.Length);

        var destPath = Path.Combine(target, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
        File.WriteAllText(destPath, "already here");

        var result = await _sut.UndoRemoveAsync(imageFile, target);

        Assert.False(result);
        Assert.Equal("already here", File.ReadAllText(destPath));
        Assert.True(File.Exists(removedPath));
    }

    public void Dispose() => _folder.Dispose();
}
