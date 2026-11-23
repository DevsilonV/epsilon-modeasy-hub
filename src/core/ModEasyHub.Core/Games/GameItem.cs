namespace ModEasyHub.Core.Games;

public enum GameStatus
{
    Found,
    NotFound
}

public sealed record GameItem(Game Game, GameStatus Status);
