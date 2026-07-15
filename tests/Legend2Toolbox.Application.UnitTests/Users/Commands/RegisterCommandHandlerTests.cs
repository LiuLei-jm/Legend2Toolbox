using FluentAssertions;
using Legend2Toolbox.Application.Common.Interfaces;
using Legend2Toolbox.Application.Feature.Identity;
using Legend2Toolbox.Domain.Models;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit.Sdk;

namespace Legend2Toolbox.Application.UnitTests.Users.Commands;

public class RegisterCommandHandlerTests
{
    private readonly IIdentityService _identityServiceMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _identityServiceMock = Substitute.For<IIdentityService>();
        _handler = new RegisterCommandHandler(_identityServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var command = new RegisterCommand("testuser", "test@example.com", "Password123!");

        _identityServiceMock.RegisterUserAsync(command).Returns(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }


    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterCommand("testuser", "test@example.com", "Password123!");
        var expectedError = "该邮箱已被注册";

        _identityServiceMock.RegisterUserAsync(command).Returns(Result.Failure(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(expectedError);
    }
}
