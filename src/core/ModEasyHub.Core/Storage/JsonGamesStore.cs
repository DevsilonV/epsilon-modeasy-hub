using System.Text.Json;
using ModEasyHub.Core.Games;

namespace ModEasyHub.Core.Storage;

public sealed class JsonGamesStore(string filePath) : IGamesStore
{
    private const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public StoreLoadResult Load()
    {
        if (!File.Exists(filePath))
            return new StoreLoadResult([], false);

        string text;
        try
        {
            text = File.ReadAllText(filePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new GamesStoreException("The games file could not be read.", ex);
        }

        var games = TryParse(text);
        if (games is not null)
            return new StoreLoadResult(games, false);

        BackUpCorruptFile();
        return new StoreLoadResult([], true);
    }

    public void Save(IReadOnlyList<Game> games)
    {
        var dto = new FileDto
        {
            SchemaVersion = CurrentSchemaVersion,
            Games = games.Select(g => new GameDto { Id = g.Id, Name = g.Name, ExePath = g.ExePath }).ToList()
        };

        var tempPath = filePath + ".tmp";
        try
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(filePath))!;
            Directory.CreateDirectory(directory);
            File.WriteAllText(tempPath, JsonSerializer.Serialize(dto, JsonOptions));
            File.Move(tempPath, filePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new GamesStoreException("The games file could not be saved.", ex);
        }
        finally
        {
            // Tras un Move correcto el .tmp ya no existe; si algo falló, no dejamos basura.
            TryDeleteTempFile(tempPath);
        }
    }

    private static void TryDeleteTempFile(string tempPath)
    {
        try
        {
            File.Delete(tempPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Limpieza de mejor esfuerzo: no debe ocultar el error original.
        }
    }

    private static List<Game>? TryParse(string text)
    {
        FileDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<FileDto>(text, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        if (dto is null || dto.SchemaVersion != CurrentSchemaVersion || dto.Games is null)
            return null;

        var games = new List<Game>();
        foreach (var entry in dto.Games)
        {
            if (entry is null
                || entry.Id == Guid.Empty
                || string.IsNullOrWhiteSpace(entry.Name)
                || string.IsNullOrWhiteSpace(entry.ExePath))
                return null;

            games.Add(new Game(entry.Id, entry.Name, entry.ExePath));
        }

        return games;
    }

    private void BackUpCorruptFile()
    {
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var backupPath = $"{filePath}.{stamp}.bak";
        for (var n = 1; File.Exists(backupPath); n++)
            backupPath = $"{filePath}.{stamp}-{n}.bak";

        try
        {
            File.Move(filePath, backupPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new GamesStoreException("The corrupt games file could not be backed up.", ex);
        }
    }

    private sealed class FileDto
    {
        public int SchemaVersion { get; set; }
        public List<GameDto>? Games { get; set; }
    }

    private sealed class GameDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? ExePath { get; set; }
    }
}
