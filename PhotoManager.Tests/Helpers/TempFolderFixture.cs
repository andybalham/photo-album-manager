namespace PhotoManager.Tests.Helpers;

public class TempFolderFixture : IDisposable
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public string CreateFolder(string relativePath)
    {
        var full = Path.Combine(Root, relativePath);
        Directory.CreateDirectory(full);
        return full;
    }

    public string CreateImageFile(string relativePath)
    {
        var full = Path.Combine(Root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);

        // Minimal valid 1x1 PNG (67 bytes)
        byte[] png =
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, // IHDR length + type
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, // width=1, height=1
            0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, // bit depth, color type, etc.
            0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, // IHDR CRC, IDAT length + type
            0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00, // IDAT data
            0x00, 0x00, 0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, // IDAT data cont.
            0x33, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, // IDAT CRC, IEND length + type
            0x44, 0xAE, 0x42, 0x60, 0x82                    // IEND data + CRC
        ];

        File.WriteAllBytes(full, png);
        return full;
    }

    public string CreateNonImageFile(string relativePath)
    {
        var full = Path.Combine(Root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, "not an image");
        return full;
    }

    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
