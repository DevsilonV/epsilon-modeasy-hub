using ModEasyHub.Core.IO;
using ModEasyHub.Core.Storage;

namespace ModEasyHub.Core.Games;

public sealed class GameLibrary(IGamesStore store, IFileSystem fileSystem)
{
    private readonly List<Game> _games = [];

    /// <summary>
    /// Carga la lista desde el almacén. Se puede volver a llamar (por ejemplo, para recargar tras un fallo).
    /// Nunca escribe en el almacén.
    /// </summary>
    public LoadOutcome Load()
    {
        StoreLoadResult result;
        try
        {
            result = store.Load();
        }
        catch (GamesStoreException)
        {
            _games.Clear();
            return LoadOutcome.Failed;
        }

        _games.Clear();
        _games.AddRange(result.Games);
        return result.RecoveredFromCorruption ? LoadOutcome.RecoveredFromCorruption : LoadOutcome.Loaded;
    }

    /// <summary>El estado se calcula en cada llamada: no se guarda.</summary>
    public IReadOnlyList<GameItem> GetGames() =>
        _games
            .Select(g => new GameItem(g, fileSystem.FileExists(g.ExePath) ? GameStatus.Found : GameStatus.NotFound))
            .ToList();
}
