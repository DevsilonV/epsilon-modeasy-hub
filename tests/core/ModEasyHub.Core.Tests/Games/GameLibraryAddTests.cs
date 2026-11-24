using ModEasyHub.Core.Games;
using ModEasyHub.Core.Tests.TestDoubles;

namespace ModEasyHub.Core.Tests.Games;

public sealed class GameLibraryAddTests
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
    public void Add_ValidExe_AddsAndPersists()
    {
        var exe = TestPaths.Exe("Alpha");
        var (lib, store, _) = Create(exe);

        var result = lib.Add(exe);

        Assert.True(result.IsSuccess);
        Assert.Equal("Alpha", result.Game!.Name);
        Assert.Single(lib.GetGames());
        Assert.Equal(1, store.SaveCalls);
        Assert.Single(store.Games);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Add_EmptyPath_FailsNotExeFile(string? path)
    {
        var (lib, _, _) = Create();

        Assert.Equal(GameLibraryError.NotExeFile, lib.Add(path!).Error);
    }

    [Fact]
    public void Add_NotExeExtension_FailsNotExeFile()
    {
        var txt = TestPaths.Exe("A").Replace(".exe", ".txt");
        var (lib, _, _) = Create(txt);

        Assert.Equal(GameLibraryError.NotExeFile, lib.Add(txt).Error);
    }

    [Fact]
    public void Add_UppercaseExeExtension_IsAccepted()
    {
        var exe = TestPaths.Exe("A").Replace(".exe", ".EXE");
        var (lib, _, _) = Create(exe);

        Assert.True(lib.Add(exe).IsSuccess);
    }

    [Fact]
    public void Add_MissingFile_FailsFileNotFound()
    {
        var (lib, _, _) = Create();

        Assert.Equal(GameLibraryError.FileNotFound, lib.Add(TestPaths.Exe("Alpha")).Error);
    }

    [Fact]
    public void Add_SameExeTwice_FailsAlreadyAdded_AndDoesNotSaveAgain()
    {
        var exe = TestPaths.Exe("Alpha");
        var (lib, store, _) = Create(exe);
        lib.Add(exe);

        var result = lib.Add(exe);

        Assert.Equal(GameLibraryError.AlreadyAdded, result.Error);
        Assert.Single(lib.GetGames());
        Assert.Equal(1, store.SaveCalls);
    }

    [Fact]
    public void Add_SamePathWithDotDotSegments_FailsAlreadyAdded()
    {
        var exe = TestPaths.Exe("Alpha");
        var folder = Path.GetDirectoryName(exe)!;
        var dotted = Path.Combine(folder, "..", Path.GetFileName(folder), "Alpha.exe");
        var normalized = Path.GetFullPath(dotted);
        var (lib, _, _) = Create(exe, normalized);
        lib.Add(exe);

        Assert.Equal(GameLibraryError.AlreadyAdded, lib.Add(dotted).Error);
    }

    [Fact]
    public void Add_SamePathDifferentCase_IsDuplicateOnWindowsOnly()
    {
        var lower = TestPaths.Exe("Alpha");
        var upper = lower.ToUpperInvariant().Replace(".EXE", ".exe");
        var (lib, _, _) = Create(lower, upper);
        lib.Add(lower);

        var result = lib.Add(upper);

        if (OperatingSystem.IsWindows())
            Assert.Equal(GameLibraryError.AlreadyAdded, result.Error);
        else
            Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Add_Failure_DoesNotChangeListOrSave()
    {
        var (lib, store, _) = Create();

        lib.Add(TestPaths.Exe("Missing"));

        Assert.Empty(lib.GetGames());
        Assert.Equal(0, store.SaveCalls);
    }

    [Fact]
    public void Add_AfterFailedLoad_FailsLoadFailed_AndDoesNotSave()
    {
        var exe = TestPaths.Exe("Alpha");
        var store = new InMemoryGamesStore { ThrowOnLoad = true };
        var lib = new GameLibrary(store, new FakeFileSystem(exe));
        lib.Load();

        var result = lib.Add(exe);

        Assert.Equal(GameLibraryError.LoadFailed, result.Error);
        Assert.Equal(0, store.SaveCalls);
        Assert.Empty(lib.GetGames());
    }

    [Fact]
    public void Add_AfterSuccessfulReload_WorksAgain()
    {
        var exe = TestPaths.Exe("Alpha");
        var store = new InMemoryGamesStore { ThrowOnLoad = true };
        var lib = new GameLibrary(store, new FakeFileSystem(exe));
        lib.Load();
        store.ThrowOnLoad = false;
        lib.Load();

        Assert.True(lib.Add(exe).IsSuccess);
    }
}
