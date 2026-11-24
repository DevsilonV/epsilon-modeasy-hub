namespace ModEasyHub.Core.IO;

public sealed class PhysicalFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);
}
