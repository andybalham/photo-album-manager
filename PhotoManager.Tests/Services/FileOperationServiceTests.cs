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
        return new ImageFile(full, relativePath, info.Name, info.CreationTime, info.Length);
    }

    // CopyToTargetAsync

    [Fact]
    public async Task CopyToTarget_CreatesFileAtCorrectPath()
    {
        var source = _folder.CreateFolder("source");
        var target = _folder.CreateFolder("target");
        var file = MakeFile(@"source\2024\photo.jpg");
        var relative = Path.GetRelativePath(source, file.FullPath);
        var imageFile = file with { RelativePath = relative };

        var result = await _sut.CopyToTargetAsync(imageFile, source, target);

        Assert.True(result);
        Assert.True(File.Exists(Path.Combine(target, relative)));
        Assert.True(File.Exists(imageFile.FullPath)); // source untouched
    }

    [Fact]
    public async Task CopyToTarget_ReturnsFalse_WhenDestinationExists()
    {
        var source = _folder.CreateFolder("source2");
        var target = _folder.CreateFolder("target2");
        var file = MakeFile(@"source2\photo.jpg");
        var relative = Path.GetRelativePath(source, file.FullPath);
        var imageFile = file with { RelativePath = relative };

        // Pre-create destination
        var destPath = Path.Combine(target, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
        File.WriteAllText(destPath, "existing");

        var result = await _sut.CopyToTargetAsync(imageFile, source, target);

        Assert.False(result);
        Assert.Equal("existing", File.ReadAllText(destPath)); // dest unchanged
        Assert.True(File.Exists(imageFile.FullPath));          // source intact
    }

    // MoveToRemovedAsync

    [Fact]
    public async Task MoveToRemoved_MovesFileToRemovedSubfolder()
    {
        var target = _folder.CreateFolder("target3");
        var file = MakeFile(@"target3\2024\photo.jpg");
        var relative = Path.GetRelativePath(target, file.FullPath);
        var imageFile = file with { RelativePath = relative };

        await _sut.MoveToRemovedAsync(imageFile, target);

        var expectedDest = Path.Combine(target, "_removed", relative);
        Assert.True(File.Exists(expectedDest));
        Assert.False(File.Exists(imageFile.FullPath)); // source gone
    }

    // UndoRemoveAsync

    [Fact]
    public async Task UndoRemove_MovesFileBackToTarget()
    {
        var target = _folder.CreateFolder("target4");
        // File lives in _removed
        var relative = Path.Combine("2024", "photo.jpg");
        var removedPath = _folder.CreateImageFile(Path.Combine("target4", "_removed", relative));
        var info = new FileInfo(removedPath);
        var imageFile = new ImageFile(removedPath, relative, info.Name, info.CreationTime, info.Length);

        var result = await _sut.UndoRemoveAsync(imageFile, target);

        Assert.True(result);
        Assert.True(File.Exists(Path.Combine(target, relative)));
        Assert.False(File.Exists(removedPath)); // _removed copy gone
    }

    [Fact]
    public async Task UndoRemove_ReturnsFalse_WhenTargetAlreadyExists()
    {
        var target = _folder.CreateFolder("target5");
        var relative = Path.Combine("2024", "photo.jpg");

        // File in _removed
        var removedPath = _folder.CreateImageFile(Path.Combine("target5", "_removed", relative));
        var info = new FileInfo(removedPath);
        var imageFile = new ImageFile(removedPath, relative, info.Name, info.CreationTime, info.Length);

        // Pre-create target file
        var destPath = Path.Combine(target, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
        File.WriteAllText(destPath, "already here");

        var result = await _sut.UndoRemoveAsync(imageFile, target);

        Assert.False(result);
        Assert.Equal("already here", File.ReadAllText(destPath));
        Assert.True(File.Exists(removedPath)); // _removed copy untouched
    }

    public void Dispose() => _folder.Dispose();
}
