using ModEasyHub.Core.Storage;

namespace ModEasyHub.Core.Tests.Storage;

public sealed class AppDataPathsTests
{
    private static string NewBase() =>
        Path.Combine(Path.GetTempPath(), "modeasyhub-tests-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void GetDataDirectory_UsesOverrideAndAppFolderName()
    {
        var @base = NewBase();

        Assert.Equal(Path.Combine(@base, "EpsilonModEasyHub"), AppDataPaths.GetDataDirectory(@base));
    }

    [Fact]
    public void GetGamesFilePath_IsGamesJsonInsideDataDirectory()
    {
        var @base = NewBase();

        Assert.Equal(Path.Combine(@base, "EpsilonModEasyHub", "games.json"), AppDataPaths.GetGamesFilePath(@base));
    }

    [Fact]
    public void GetDataDirectory_DefaultIsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(AppDataPaths.GetDataDirectory()));
    }

    [Fact]
    public void GetDataDirectory_DoesNotCreateDirectory()
    {
        var @base = NewBase();

        AppDataPaths.GetDataDirectory(@base);
        AppDataPaths.GetGamesFilePath(@base);

        Assert.False(Directory.Exists(@base));
    }
}
