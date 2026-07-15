using FluentAssertions;
using Legend2Toolbox.Application.Common.Interfaces;
using Legend2Toolbox.Application.Feature.Identity;
using Legend2Toolbox.Domain.Models;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Legend2Toolbox.Application.UnitTests.Users.Queries;

public class LoginQueryHandlerTests
{
    private readonly IIdentityService _identityServiceMock;
    private readonly LoginQueryHandler _handler;

    public LoginQueryHandlerTests()
    {
        _identityServiceMock = Substitute.For<IIdentityService>();
        _handler = new LoginQueryHandler(_identityServiceMock); ;
    }


    [Fact]
    public async Task Handle_ShouldReturnClaimsPrincipal_WhenCredentialsAreValid()
    {
        // Arrange
        var query = new LoginQuery("admin", "Password123!");
        var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity("Bearer"));

        _identityServiceMock.AuthenticateUserAsync(query)
            .Returns(Result<ClaimsPrincipal>.Success(expectedPrincipal));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedPrincipal);
    }


    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCredentialsAreInvalid()
    {
        // Arrange
        var query = new LoginQuery("admin", "WrongPassword!");
        var expectedError = "用户名或密码错误";

        _identityServiceMock.AuthenticateUserAsync(query)
            .Returns(Result<ClaimsPrincipal>.Failure(expectedError));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(expectedError);
    }


}
