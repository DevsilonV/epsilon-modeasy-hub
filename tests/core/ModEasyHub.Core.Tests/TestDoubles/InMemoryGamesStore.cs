using ModEasyHub.Core.Games;
using ModEasyHub.Core.Storage;

namespace ModEasyHub.Core.Tests.TestDoubles;

public sealed class InMemoryGamesStore(params Game[] initial) : IGamesStore
{
    public List<Game> Games { get; } = [.. initial];

    public bool RecoveredFromCorruption { get; set; }

    public bool ThrowOnLoad { get; set; }

    public bool ThrowOnSave { get; set; }

    public int SaveCalls { get; private set; }

    public StoreLoadResult Load()
    {
        if (ThrowOnLoad)
            throw new GamesStoreException("Load failed (test double).");
        return new StoreLoadResult([.. Games], RecoveredFromCorruption);
    }

    public void Save(IReadOnlyList<Game> games)
    {
        SaveCalls++;
        if (ThrowOnSave)
            throw new GamesStoreException("Save failed (test double).");
        Games.Clear();
        Games.AddRange(games);
    }
}
