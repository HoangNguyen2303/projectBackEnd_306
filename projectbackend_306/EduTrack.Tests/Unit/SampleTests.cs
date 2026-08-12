using EduTrack.Application.Common;
using Xunit;

namespace EduTrack.Tests.Unit;

public class SampleTests
{
    [Fact]
    public void ApiResponse_Ok_SetsSuccessTrue()
    {
        var res = ApiResponse<string>.Ok("hi");
        Assert.True(res.Success);
        Assert.Equal(200, res.StatusCode);
        Assert.Equal("hi", res.Data);
    }
}
