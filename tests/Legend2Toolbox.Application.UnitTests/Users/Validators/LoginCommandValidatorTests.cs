namespace Legend2Toolbox.Application.UnitTests.Users.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }


    [Fact]
    public void Validator_ShouldHaveError_WhenUsernameIsInvalid()
    {
        // Arrange
        var command = new LoginCommand("", "Password123!");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("为空"));
    }


    [Fact]
    public void Validator_ShouldHaveError_WhenPasswordIsInvalid()
    {
        // Arrange
        var command = new LoginCommand("test_user", "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("为空"));
    }



    [Fact]
    public void Validator_ShouldAreValid_WhenAllFieldsAreCorrect()
    {
        // Arrange
        var command = new LoginCommand("test_user", "Password123!");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }



}
