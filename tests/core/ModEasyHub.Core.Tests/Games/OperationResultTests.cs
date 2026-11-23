using ModEasyHub.Core.Games;

namespace ModEasyHub.Core.Tests.Games;

public class OperationResultTests
{
    [Fact]
    public void Success_IsSuccessAndHasNoError()
    {
        var r = OperationResult.Success();
        Assert.True(r.IsSuccess);
        Assert.Null(r.Error);
    }

    [Fact]
    public void Failure_IsNotSuccessAndKeepsError()
    {
        var r = OperationResult.Failure(GameLibraryError.AlreadyAdded);
        Assert.False(r.IsSuccess);
        Assert.Equal(GameLibraryError.AlreadyAdded, r.Error);
    }
}
