namespace ModEasyHub.Gui.Tests.TestDoubles;

public static class TestPaths
{
    public static string Exe(string name) =>
        OperatingSystem.IsWindows()
            ? $@"C:\Games\{name}\{name}.exe"
            : $"/games/{name}/{name}.exe";
}
