using ModEasyHub.Core.Games;
using ModEasyHub.Core.IO;
using ModEasyHub.Core.Tests.TestDoubles;

namespace ModEasyHub.Core.Tests.Games;

public sealed class GameLibraryRemoveTests
{
    private static (GameLibrary Lib, InMemoryGamesStore Store, FakeFileSystem Fs) Create(params string[] existingFiles)
    {
        var store = new InMemoryGamesStore();
        var fs = new FakeFileSystem(existingFiles);
        var lib = new GameLibrary(store, fs);
        lib.Load();
        return (lib, store, fs);
    }

    [Fact]
    public void Remove_ExistingGame_RemovesAndPersists()
    {
        var exe = TestPaths.Exe("Alpha");
        var (lib, store, _) = Create(exe);
        var game = lib.Add(exe).Game!;

        var result = lib.Remove(game.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(game.Id, result.Game!.Id);
        Assert.Empty(lib.GetGames());
        Assert.Empty(store.Games);
        Assert.Equal(2, store.SaveCalls);
    }

    [Fact]
    public void Remove_UnknownId_FailsGameNotInList_WithoutSaving()
    {
        var (lib, store, _) = Create();

        var result = lib.Remove(Guid.NewGuid());

        Assert.Equal(GameLibraryError.GameNotInList, result.Error);
        Assert.Equal(0, store.SaveCalls);
    }

    [Fact]
    public void Remove_OnlyRemovesTheRequestedGame()
    {
        var alpha = TestPaths.Exe("Alpha");
        var beta = TestPaths.Exe("Beta");
        var (lib, _, _) = Create(alpha, beta);
        var a = lib.Add(alpha).Game!;
        lib.Add(beta);

        lib.Remove(a.Id);

        var remaining = Assert.Single(lib.GetGames());
        Assert.Equal("Beta", remaining.Game.Name);
    }

    [Fact]
    public void Remove_NotFoundGame_CanBeRemoved()
    {
        var exe = TestPaths.Exe("Alpha");
        var (lib, _, fs) = Create(exe);
        var game = lib.Add(exe).Game!;
        fs.Remove(exe);

        var result = lib.Remove(game.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(lib.GetGames());
    }

    [Fact]
    public void Remove_ThenAddSameExe_Succeeds()
    {
        var exe = TestPaths.Exe("Alpha");
        var (lib, _, _) = Create(exe);
        var game = lib.Add(exe).Game!;
        lib.Remove(game.Id);

        Assert.True(lib.Add(exe).IsSuccess);
    }

    [Fact]
    public void Remove_AfterFailedLoad_FailsLoadFailed_AndDoesNotSave()
    {
        var store = new InMemoryGamesStore { ThrowOnLoad = true };
        var lib = new GameLibrary(store, new FakeFileSystem());
        lib.Load();

        var result = lib.Remove(Guid.NewGuid());

        Assert.Equal(GameLibraryError.LoadFailed, result.Error);
        Assert.Equal(0, store.SaveCalls);
    }

    [Fact]
    public void Remove_AfterSuccessfulReload_WorksAgain()
    {
        var exe = TestPaths.Exe("Alpha");
        var game = Game.FromExePath(exe);
        var store = new InMemoryGamesStore(game) { ThrowOnLoad = true };
        var lib = new GameLibrary(store, new FakeFileSystem(exe));
        lib.Load();
        store.ThrowOnLoad = false;
        lib.Load();

        Assert.True(lib.Remove(game.Id).IsSuccess);
    }

    [Fact]
    public void Remove_DoesNotTouchGameFiles()
    {
        var dir = Path.Combine(Path.GetTempPath(), "modeasyhub-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var exe = Path.Combine(dir, "Game.exe");
            File.WriteAllBytes(exe, []);
            var lib = new GameLibrary(new InMemoryGamesStore(), new PhysicalFileSystem());
            lib.Load();
            var game = lib.Add(exe).Game!;

            lib.Remove(game.Id);

            Assert.True(File.Exists(exe));
            Assert.Equal([exe], Directory.GetFileSystemEntries(dir));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
