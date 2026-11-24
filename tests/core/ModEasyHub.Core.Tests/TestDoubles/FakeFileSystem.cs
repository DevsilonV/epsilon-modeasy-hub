using ModEasyHub.Core.IO;

namespace ModEasyHub.Core.Tests.TestDoubles;

public sealed class FakeFileSystem(params string[] existing) : IFileSystem
{
    private readonly HashSet<string> _files = new(existing);

    public void Add(string path) => _files.Add(path);

    public void Remove(string path) => _files.Remove(path);

    public bool FileExists(string path) => _files.Contains(path);
}
