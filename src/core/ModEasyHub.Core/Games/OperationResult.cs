namespace ModEasyHub.Core.Games;

public sealed record OperationResult(GameLibraryError? Error, Game? Game)
{
    public bool IsSuccess => Error is null;

    public static OperationResult Success(Game? game = null) => new(null, game);

    public static OperationResult Failure(GameLibraryError error, Game? game = null) => new(error, game);
}
