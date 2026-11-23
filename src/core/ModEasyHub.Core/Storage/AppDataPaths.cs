namespace ModEasyHub.Core.Storage;

/// <summary>
/// Calcula dónde guarda la app sus datos. Solo calcula rutas: no crea carpetas.
/// </summary>
public static class AppDataPaths
{
    public const string AppFolderName = "EpsilonModEasyHub";

    private const string GamesFileName = "games.json";

    public static string GetDataDirectory(string? baseOverride = null)
    {
        var baseDirectory = baseOverride
            ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(baseDirectory, AppFolderName);
    }

    public static string GetGamesFilePath(string? baseOverride = null) =>
        Path.Combine(GetDataDirectory(baseOverride), GamesFileName);
}
