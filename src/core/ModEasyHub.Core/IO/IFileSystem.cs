namespace ModEasyHub.Core.IO;

/// <summary>Solo lectura a propósito: core nunca escribe ni borra archivos de un juego.</summary>
public interface IFileSystem
{
    bool FileExists(string path);
}
