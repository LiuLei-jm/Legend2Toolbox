using FluentValidation.TestHelper;

namespace Legend2Toolbox.Application.UnitTests.Users.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }


    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Should_Have_Error_When_Username_Is_Invalid(string invalidUsername)
    {
        // Arrange
        var command = new RegisterCommand(invalidUsername, "valid@example.com", "Password123!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }


    [Theory]
    [InlineData("")]
    [InlineData("plainaddress")]
    [InlineData("@no-local-part.com")]
    public void Should_Have_Error_When_Eamil_Is_Invalid(string invalidEamil)
    {
        // Arrange
        var command = new RegisterCommand("valid_user", invalidEamil, "Password123!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }



    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void Should_Have_Error_When_Password_Is_Invalid(string invalidPassword)
    {
        // Arrange
        var command = new RegisterCommand("valid_user", "valid@example.com", invalidPassword);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
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
