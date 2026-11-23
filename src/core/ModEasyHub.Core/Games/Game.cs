namespace ModEasyHub.Core.Games;

public sealed record Game(Guid Id, string Name, string ExePath)
{
    public string FolderPath => Path.GetDirectoryName(ExePath) ?? string.Empty;

    public static Game FromExePath(string exePath)
    {
        var fullPath = Path.GetFullPath(exePath);
        return new Game(Guid.NewGuid(), Path.GetFileNameWithoutExtension(fullPath), fullPath);
    }
}
