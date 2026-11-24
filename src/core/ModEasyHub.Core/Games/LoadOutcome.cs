namespace ModEasyHub.Core.Games;

public enum LoadOutcome
{
    /// <summary>Lista cargada con normalidad (o primera ejecución, sin archivo).</summary>
    Loaded,

    /// <summary>El archivo estaba corrupto: se guardó como copia .bak y la lista arranca vacía.</summary>
    RecoveredFromCorruption,

    /// <summary>El archivo no se pudo leer. No se toca; la lista queda vacía hasta recargar.</summary>
    Failed
}
