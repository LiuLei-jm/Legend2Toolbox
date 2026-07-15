using FluentAssertions;
using Legend2Toolbox.Application.Feature.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Legend2Toolbox.Application.UnitTests.Users.Commands;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }


    [Fact]
    public void Validate_ShouldHaveError_WhenEmailIsInvalid()
    {
        // Arrange
        var command = new RegisterCommand("john_doe", "invalid-email", "Password123!");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("有效"));
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPasswordIsTooShort()
    {
        // Arrange
        var command = new RegisterCommand("john_doe", "test@example.com", "123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllFieldsAreCorrect()
    {
        // Arrange
        var command = new RegisterCommand("john_doe", "test@example.com", "Password123!");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
