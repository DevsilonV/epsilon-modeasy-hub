using ModEasyHub.Core.Games;
using ModEasyHub.Core.Tests.TestDoubles;

namespace ModEasyHub.Core.Tests.Games;

public class GameTests
{
    [Fact]
    public void FromExePath_DeducesNameWithoutExtension()
    {
        var g = Game.FromExePath(TestPaths.Exe("Alpha"));
        Assert.Equal("Alpha", g.Name);
    }

    [Fact]
    public void FromExePath_FolderPathIsDirectoryOfExe()
    {
        var p = TestPaths.Exe("Alpha");
        Assert.Equal(Path.GetDirectoryName(p), Game.FromExePath(p).FolderPath);
    }

    [Fact]
    public void FromExePath_GeneratesDistinctIds()
    {
        var p = TestPaths.Exe("Alpha");
        Assert.NotEqual(Game.FromExePath(p).Id, Game.FromExePath(p).Id);
    }
}
