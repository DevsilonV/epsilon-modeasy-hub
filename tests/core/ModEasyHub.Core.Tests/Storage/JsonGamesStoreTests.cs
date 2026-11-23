using ModEasyHub.Core.Games;
using ModEasyHub.Core.Storage;
using ModEasyHub.Core.Tests.TestDoubles;

namespace ModEasyHub.Core.Tests.Storage;

public sealed class JsonGamesStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "modeasyhub-tests-" + Guid.NewGuid().ToString("N"));

    private string FilePath => Path.Combine(_dir, "games.json");

    private JsonGamesStore Store() => new(FilePath);

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    [Fact]
    public void Load_NoFile_ReturnsEmptyNotRecovered()
    {
        var r = Store().Load();
        Assert.Empty(r.Games);
        Assert.False(r.RecoveredFromCorruption);
    }

    [Fact]
    public void SaveThenLoad_RoundTripsGames()
    {
        var g = Game.FromExePath(TestPaths.Exe("Alpha"));
        Store().Save([g]);

        var r = Store().Load();

        Assert.Equal(new[] { g }, r.Games);
        Assert.False(r.RecoveredFromCorruption);
    }

    [Fact]
    public void Save_CreatesMissingDirectory()
    {
        Assert.False(Directory.Exists(_dir));
        Store().Save([]);
        Assert.True(File.Exists(FilePath));
    }

    [Theory]
    [InlineData("not json {{{")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{\"schemaVersion\":1}")]
    [InlineData("{\"schemaVersion\":99,\"games\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"games\":[{\"id\":\"00000000-0000-0000-0000-000000000000\",\"name\":\"\",\"exePath\":\"\"}]}")]
    public void Load_Corrupt_BacksUpAndReturnsEmpty(string content)
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, content);

        var r = Store().Load();

        Assert.Empty(r.Games);
        Assert.True(r.RecoveredFromCorruption);
        Assert.False(File.Exists(FilePath));
        var backup = Assert.Single(Directory.GetFiles(_dir, "*.bak"));
        Assert.Equal(content, File.ReadAllText(backup));
    }

    [Fact]
    public void Save_AfterCorruptRecovery_DoesNotTouchBackup()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, "not json {{{");
        var store = Store();
        store.Load();

        store.Save([Game.FromExePath(TestPaths.Exe("Alpha"))]);

        var backup = Assert.Single(Directory.GetFiles(_dir, "*.bak"));
        Assert.Equal("not json {{{", File.ReadAllText(backup));
        Assert.True(File.Exists(FilePath));
    }

    [Fact]
    public void Save_WhenMoveFails_ThrowsAndLeavesNoTmpFile()
    {
        // Un directorio en el lugar de games.json hace fallar File.Move tras escribir el .tmp
        Directory.CreateDirectory(FilePath);

        Assert.Throws<GamesStoreException>(() => Store().Save([Game.FromExePath(TestPaths.Exe("Alpha"))]));

        Assert.False(File.Exists(FilePath + ".tmp"));
    }
}
