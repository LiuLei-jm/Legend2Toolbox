using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legend2Toolbox.Application.UnitTests.Users.Commands;

public class ForgotPassworCommandHandlerTests
{
    private readonly IIdentityService _identityServiceMock;
    private readonly ForgotPasswordCommandHandler _handler;
    public ForgotPassworCommandHandlerTests()
    {
        _identityServiceMock = Substitute.For<IIdentityService>();
        _handler = new ForgotPasswordCommandHandler(_identityServiceMock);
    }


    [Fact]
    public async Task Handler_ShouldReturnSuccess_ToPreventEmailEnumeration()
    {
        // Arrange
        var command = new ForgotPasswordCommand("any_email@example.com", "http://reset.link");
        _identityServiceMock.ForgotPasswordAsync(command).Returns(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }


}
