using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legend2Toolbox.Application.UnitTests.Users.Validators;

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator;

    public ChangePasswordCommandValidatorTests()
    {
        _validator = new ChangePasswordCommandValidator();
    }


    [Fact]
    public void Should_Have_Error_When_NewPassword_Equals_OldPassword()
    {
        // Arrange
        var command = new ChangePasswordCommand("SamePassword123!","SamePassword123!");

        // Act
        var result= _validator.TestValidate(command);


        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword).WithErrorMessage("新密码不能与旧密码相同");
    }



    [Fact]
    public void Should_Not_Have_Error_When_Passwords_Are_Valid_And_Different()
    {
        // Arrange
        var command = new ChangePasswordCommand("OldPassword123!", "NewPassword123!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }


}
