using Legend2Toolbox.Application.Feature.Admin;
using Legend2Toolbox.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legend2Toolbox.Application.UnitTests.Users.Commands;

public class DeleteUserCommandTests
{
    private readonly IIdentityService _identityServiceMock;
    private readonly DeleteUserCommandHandler _handler;
    public DeleteUserCommandTests()
    {
        _identityServiceMock = Substitute.For<IIdentityService>();
        _handler = new DeleteUserCommandHandler(_identityServiceMock);
    }


    [Fact]
    public async Task Handler_ShouldReturnSuccess_WhenUserIsDeletedSuccessFully
    ()
    {
        // Arrange
        var command = new DeleteUserCommand("normal_user_123");

        _identityServiceMock.DeleteUserAsync(command.UserId).Returns(Result.Success());
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }



    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTryingToDeleteSuperAdmin()
    {
        // Arrange
        var command = new DeleteUserCommand("super_admin_id");
        var expectedError = "超级管理员无法删除";

        _identityServiceMock.DeleteUserAsync(command.UserId).Returns(Result.Failure(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(expectedError);
    }


}
