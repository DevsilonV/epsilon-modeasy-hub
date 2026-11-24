using ModEasyHub.Core.Games;
using ModEasyHub.Core.Tests.TestDoubles;

namespace ModEasyHub.Core.Tests.Games;

public sealed class GameLibraryLoadTests
{
    private static readonly Game Alpha = Game.FromExePath(TestPaths.Exe("Alpha"));
    private static readonly Game Beta = Game.FromExePath(TestPaths.Exe("Beta"));

    [Fact]
    public void GetGames_BeforeLoad_IsEmpty()
    {
        var lib = new GameLibrary(new InMemoryGamesStore(Alpha), new FakeFileSystem(Alpha.ExePath));

        Assert.Empty(lib.GetGames());
    }

    [Fact]
    public void Load_ReturnsStoredGames_WithFoundStatus()
    {
        var lib = new GameLibrary(new InMemoryGamesStore(Alpha), new FakeFileSystem(Alpha.ExePath));

        var outcome = lib.Load();

        Assert.Equal(LoadOutcome.Loaded, outcome);
        var item = Assert.Single(lib.GetGames());
        Assert.Equal(Alpha, item.Game);
        Assert.Equal(GameStatus.Found, item.Status);
    }

    [Fact]
    public void GetGames_MissingExe_IsNotFoundButStillListed()
    {
        var lib = new GameLibrary(new InMemoryGamesStore(Alpha), new FakeFileSystem());
        lib.Load();

        Assert.Equal(GameStatus.NotFound, Assert.Single(lib.GetGames()).Status);
    }

    [Fact]
    public void GetGames_StatusFollowsFileSystemChanges()
    {
        var fs = new FakeFileSystem(Alpha.ExePath);
        var lib = new GameLibrary(new InMemoryGamesStore(Alpha), fs);
        lib.Load();
        Assert.Equal(GameStatus.Found, Assert.Single(lib.GetGames()).Status);

        fs.Remove(Alpha.ExePath);

        Assert.Equal(GameStatus.NotFound, Assert.Single(lib.GetGames()).Status);
    }

    [Fact]
    public void Load_WhenStoreRecoveredFromCorruption_ReturnsRecoveredAndEmptyList()
    {
        var store = new InMemoryGamesStore { RecoveredFromCorruption = true };
        var lib = new GameLibrary(store, new FakeFileSystem());

        Assert.Equal(LoadOutcome.RecoveredFromCorruption, lib.Load());
        Assert.Empty(lib.GetGames());
    }

    [Fact]
    public void Load_NeverModifiesStore()
    {
        var store = new InMemoryGamesStore(Alpha);
        var lib = new GameLibrary(store, new FakeFileSystem(Alpha.ExePath));

        lib.Load();

        Assert.Equal(0, store.SaveCalls);
    }

    [Fact]
    public void Load_WhenStoreCannotBeRead_ReturnsFailedAndDoesNotThrow()
    {
        var store = new InMemoryGamesStore(Alpha) { ThrowOnLoad = true };
        var lib = new GameLibrary(store, new FakeFileSystem(Alpha.ExePath));

        Assert.Equal(LoadOutcome.Failed, lib.Load());
        Assert.Empty(lib.GetGames());
        Assert.Equal(0, store.SaveCalls);
    }

    [Fact]
    public void Load_CalledAgainAfterFailure_Reloads()
    {
        var store = new InMemoryGamesStore(Alpha) { ThrowOnLoad = true };
        var lib = new GameLibrary(store, new FakeFileSystem(Alpha.ExePath));
        lib.Load();

        store.ThrowOnLoad = false;

        Assert.Equal(LoadOutcome.Loaded, lib.Load());
        Assert.Single(lib.GetGames());
    }

    [Fact]
    public void Load_CalledTwice_DoesNotDuplicateGames()
    {
        var lib = new GameLibrary(new InMemoryGamesStore(Alpha, Beta), new FakeFileSystem());

        lib.Load();
        lib.Load();

        Assert.Equal(2, lib.GetGames().Count);
    }
}
