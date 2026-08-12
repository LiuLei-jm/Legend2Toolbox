namespace Legend2Toolbox.Application.UnitTests.Users.Commands;

public class LoginCommandHandlerTests
{
    private readonly LoginCommandHandler _handler;
    private readonly IIdentityService _identityServiceMock;

    public LoginCommandHandlerTests()
    {
        _identityServiceMock = Substitute.For<IIdentityService>();
        _handler = new LoginCommandHandler(_identityServiceMock);
    }


    [Fact]
    public async Task Handle_ShouldReturnAuthResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginCommand("admin", "Password123!");
        var expectedResponse = new AuthResponse(
            AccessToken: "mock_access_token",
            RefreshToken: "mock_refresh_token",
            ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15)
        );

        _identityServiceMock.LoginUserAsync(command)
            .Returns(Result<AuthResponse>.Success(expectedResponse));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedResponse);
    }


    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCredentialsAreInvalid()
    {
        // Arrange
        var command = new LoginCommand("admin", "WrongPassword!");
        var expectedError = "用户名或密码错误";

        _identityServiceMock.LoginUserAsync(command)
            .Returns(Result<AuthResponse>.Failure(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(expectedError);
    }
}