using ModEasyHub.Core.IO;
using ModEasyHub.Core.Storage;

namespace ModEasyHub.Core.Games;

public sealed class GameLibrary(IGamesStore store, IFileSystem fileSystem)
{
    private static readonly StringComparison PathComparison =
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private readonly List<Game> _games = [];
    private bool _loadFailed;

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
            _loadFailed = true;
            return LoadOutcome.Failed;
        }

        _games.Clear();
        _games.AddRange(result.Games);
        _loadFailed = false;
        return result.RecoveredFromCorruption ? LoadOutcome.RecoveredFromCorruption : LoadOutcome.Loaded;
    }

    /// <summary>El estado se calcula en cada llamada: no se guarda.</summary>
    public IReadOnlyList<GameItem> GetGames() =>
        _games
            .Select(g => new GameItem(g, fileSystem.FileExists(g.ExePath) ? GameStatus.Found : GameStatus.NotFound))
            .ToList();

    /// <summary>
    /// Añade un juego a partir de la ruta de su .exe y guarda la lista.
    /// Se rechaza si la última carga falló: guardar sobrescribiría un archivo que no se pudo leer.
    /// Orden de validación: la primera comprobación que falla decide el error.
    /// </summary>
    public OperationResult Add(string exePath)
    {
        if (_loadFailed)
            return OperationResult.Failure(GameLibraryError.LoadFailed);

        if (string.IsNullOrWhiteSpace(exePath)
            || !string.Equals(Path.GetExtension(exePath), ".exe", StringComparison.OrdinalIgnoreCase))
            return OperationResult.Failure(GameLibraryError.NotExeFile);

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(exePath);
        }
        catch (Exception e) when (e is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return OperationResult.Failure(GameLibraryError.NotExeFile);
        }

        if (!fileSystem.FileExists(fullPath))
            return OperationResult.Failure(GameLibraryError.FileNotFound);

        if (_games.Any(g => string.Equals(g.ExePath, fullPath, PathComparison)))
            return OperationResult.Failure(GameLibraryError.AlreadyAdded);

        var game = Game.FromExePath(fullPath);
        _games.Add(game);
        return Persist(game);
    }

    /// <summary>
    /// Quita un juego de la lista y guarda. Solo toca la lista de la app: nunca archivos del juego.
    /// Se rechaza si la última carga falló (mismo motivo que en Add).
    /// </summary>
    public OperationResult Remove(Guid id)
    {
        if (_loadFailed)
            return OperationResult.Failure(GameLibraryError.LoadFailed);

        var game = _games.FirstOrDefault(g => g.Id == id);
        if (game is null)
            return OperationResult.Failure(GameLibraryError.GameNotInList);

        _games.Remove(game);
        return Persist(game);
    }

    /// <summary>
    /// Guarda la lista completa. Si falla, el cambio se mantiene en memoria y se devuelve SaveFailed
    /// con el juego afectado; la siguiente operación vuelve a intentar guardar la lista entera.
    /// </summary>
    private OperationResult Persist(Game affected)
    {
        try
        {
            store.Save(_games);
        }
        catch (GamesStoreException)
        {
            return OperationResult.Failure(GameLibraryError.SaveFailed, affected);
        }

        return OperationResult.Success(affected);
    }
}
