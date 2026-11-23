using ModEasyHub.Core.Games;

namespace ModEasyHub.Core.Storage;

public interface IGamesStore
{
    StoreLoadResult Load();

    void Save(IReadOnlyList<Game> games);
}

public sealed record StoreLoadResult(IReadOnlyList<Game> Games, bool RecoveredFromCorruption);

public sealed class GamesStoreException(string message, Exception? inner = null) : Exception(message, inner);
