namespace Legend2Toolbox.Application.UnitTests.Users.Commands;

public class LoginCommandHandlerTests
{
    private readonly IIdentityService _identityServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _identityServiceMock = Substitute.For<IIdentityService>();
        _handler = new LoginCommandHandler(_identityServiceMock); 
    }


    [Fact]
    public async Task Handle_ShouldReturnClaimsPrincipal_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginCommand("admin", "Password123!");
        var expectedPrincipal = new ClaimsPrincipal(new ClaimsIdentity("Bearer"));

        _identityServiceMock.AuthenticateUserAsync(command)
            .Returns(Result<ClaimsPrincipal>.Success(expectedPrincipal));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedPrincipal);
    }


    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCredentialsAreInvalid()
    {
        // Arrange
        var query = new LoginCommand("admin", "WrongPassword!");
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
