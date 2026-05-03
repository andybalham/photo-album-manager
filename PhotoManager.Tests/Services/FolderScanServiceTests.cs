using PhotoManager.Services;
using PhotoManager.Tests.Helpers;

namespace PhotoManager.Tests.Services;

public class FolderScanServiceTests : IDisposable
{
    private readonly TempFolderFixture _folder = new();
    private readonly FolderScanService _sut = new();

    // GetFilesInFolderAsync

    [Fact]
    public async Task GetFilesInFolder_EmptyFolder_ReturnsEmpty()
    {
        _folder.CreateFolder("empty");
        var result = await _sut.GetFilesInFolderAsync(
            Path.Combine(_folder.Root, "empty"), _folder.Root);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFilesInFolder_ExcludesNonImageFiles()
    {
        _folder.CreateNonImageFile("sub\\readme.txt");
        _folder.CreateImageFile("sub\\photo.jpg");

        var result = await _sut.GetFilesInFolderAsync(
            Path.Combine(_folder.Root, "sub"), _folder.Root);

        Assert.Single(result);
        Assert.Equal("photo.jpg", result[0].FileName);
    }

    [Fact]
    public async Task GetFilesInFolder_RelativePathIsRelativeToRoot()
    {
        _folder.CreateImageFile(@"2024\Holiday\photo.png");

        var result = await _sut.GetFilesInFolderAsync(
            Path.Combine(_folder.Root, "2024", "Holiday"), _folder.Root);

        Assert.Single(result);
        Assert.Equal(Path.Combine("2024", "Holiday", "photo.png"), result[0].RelativePath);
    }

    [Fact]
    public async Task GetFilesInFolder_DoesNotReturnFilesFromSubfolders()
    {
        _folder.CreateImageFile(@"parent\photo.jpg");
        _folder.CreateImageFile(@"parent\child\other.jpg");

        var result = await _sut.GetFilesInFolderAsync(
            Path.Combine(_folder.Root, "parent"), _folder.Root);

        Assert.Single(result);
        Assert.Equal("photo.jpg", result[0].FileName);
    }

    // GetImageSubfoldersAsync

    [Fact]
    public async Task GetImageSubfolders_ExcludesRemovedFolder()
    {
        _folder.CreateImageFile(@"_removed\photo.jpg");
        _folder.CreateImageFile(@"normal\photo.jpg");

        var result = await _sut.GetImageSubfoldersAsync(_folder.Root);

        Assert.DoesNotContain(result, p => Path.GetFileName(p)
            .Equals("_removed", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result, p => Path.GetFileName(p) == "normal");
    }

    [Fact]
    public async Task GetImageSubfolders_ExcludesFoldersWithNoImages()
    {
        _folder.CreateFolder("empty");
        _folder.CreateImageFile(@"hasimage\photo.jpg");

        var result = await _sut.GetImageSubfoldersAsync(_folder.Root);

        Assert.DoesNotContain(result, p => Path.GetFileName(p) == "empty");
        Assert.Contains(result, p => Path.GetFileName(p) == "hasimage");
    }

    [Fact]
    public async Task GetImageSubfolders_IncludesFolderWithImageInDescendant()
    {
        _folder.CreateImageFile(@"parent\child\photo.jpg");

        var result = await _sut.GetImageSubfoldersAsync(_folder.Root);

        Assert.Contains(result, p => Path.GetFileName(p) == "parent");
    }

    // GetImageCountAsync

    [Fact]
    public async Task GetImageCount_ReturnsCorrectCount()
    {
        _folder.CreateImageFile(@"counted\a.jpg");
        _folder.CreateImageFile(@"counted\b.png");
        _folder.CreateNonImageFile(@"counted\c.txt");
        _folder.CreateImageFile(@"counted\sub\d.jpg"); // not direct — should not count

        var count = await _sut.GetImageCountAsync(Path.Combine(_folder.Root, "counted"));

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetImageCount_EmptyFolder_ReturnsZero()
    {
        _folder.CreateFolder("empty2");
        var count = await _sut.GetImageCountAsync(Path.Combine(_folder.Root, "empty2"));
        Assert.Equal(0, count);
    }

    public void Dispose() => _folder.Dispose();
}
