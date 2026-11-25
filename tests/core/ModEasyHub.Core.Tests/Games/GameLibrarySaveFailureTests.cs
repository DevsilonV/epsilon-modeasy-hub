using ModEasyHub.Core.Games;
using ModEasyHub.Core.Storage;
using ModEasyHub.Core.Tests.TestDoubles;

namespace ModEasyHub.Core.Tests.Games;

public sealed class GameLibrarySaveFailureTests
{
    [Fact]
    public void Add_WhenSaveFails_ReturnsSaveFailed_AndKeepsGameInMemory()
    {
        var exe = TestPaths.Exe("Alpha");
        var store = new InMemoryGamesStore { ThrowOnSave = true };
        var lib = new GameLibrary(store, new FakeFileSystem(exe));
        lib.Load();

        var result = lib.Add(exe);

        Assert.Equal(GameLibraryError.SaveFailed, result.Error);
        Assert.NotNull(result.Game);
        Assert.Single(lib.GetGames());
    }

    [Fact]
    public void Remove_WhenSaveFails_ReturnsSaveFailed_AndGameStaysRemoved()
    {
        var exe = TestPaths.Exe("Alpha");
        var store = new InMemoryGamesStore();
        var lib = new GameLibrary(store, new FakeFileSystem(exe));
        lib.Load();
        var game = lib.Add(exe).Game!;
        store.ThrowOnSave = true;

        var result = lib.Remove(game.Id);

        Assert.Equal(GameLibraryError.SaveFailed, result.Error);
        Assert.Equal(game.Id, result.Game!.Id);
        Assert.Empty(lib.GetGames());
    }

    [Fact]
    public void NextOperation_AfterFailedSave_SavesFullList()
    {
        var alpha = TestPaths.Exe("Alpha");
        var beta = TestPaths.Exe("Beta");
        var store = new InMemoryGamesStore { ThrowOnSave = true };
        var lib = new GameLibrary(store, new FakeFileSystem(alpha, beta));
        lib.Load();
        lib.Add(alpha);
        store.ThrowOnSave = false;

        var result = lib.Add(beta);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, store.Games.Count);
    }

    [Fact]
    public void JsonStore_UnwritableDirectory_ThrowsGamesStoreException()
    {
        var blocker = Path.Combine(Path.GetTempPath(), "modeasyhub-tests-" + Guid.NewGuid().ToString("N"));
        File.WriteAllText(blocker, "not a directory");
        try
        {
            var store = new JsonGamesStore(Path.Combine(blocker, "games.json"));

            Assert.Throws<GamesStoreException>(() => store.Save([Game.FromExePath(TestPaths.Exe("Alpha"))]));
        }
        finally
        {
            File.Delete(blocker);
        }
    }
}
