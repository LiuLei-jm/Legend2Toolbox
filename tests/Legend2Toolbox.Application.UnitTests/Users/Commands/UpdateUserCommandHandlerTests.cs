using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legend2Toolbox.Application.UnitTests.Users.Commands;

public class UpdateUserCommandHandlerTests
{
    private readonly IIdentityService _identityServiceMock;
    private readonly UpdateUserCommandHandler _handler;
    public UpdateUserCommandHandlerTests()
    {
        _identityServiceMock = Substitute.For<IIdentityService>();
        _handler = new UpdateUserCommandHandler(_identityServiceMock);
    }


    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUpdateIsSuccessful()
    {
        // Arrange
        var command = new UpdateUserCommand("user_123", "new_username", "new@example.com");
        _identityServiceMock.UpdateUserAsync(command).Returns(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }



    [Fact]
    public async Task Handle_SHouldReturnFailure_WhenEmailOrUsernameAlradyExists()
    {
        // Arrange
        var command = new UpdateUserCommand("user_123", "taken_username", "take@example.com");
        var expectedError = "该邮箱或用户名已被其他用户占用";

        _identityServiceMock.UpdateUserAsync(command).Returns(Result.Failure(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(expectedError);
    }


}
