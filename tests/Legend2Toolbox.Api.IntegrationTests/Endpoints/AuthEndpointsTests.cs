namespace Legend2Toolbox.Api.IntegrationTests.Endpoints;

[Collection("Integration Tests")]
public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AuthEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }


    [Fact]
    public async Task Register_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var request = new RegisterRequest("new_integration_user", "test@example.com", "SecurePassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        // Arrange
        var request = new RegisterRequest("bad_user", "not-an-email", "SecurePassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("有效");
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var username = "loginuser";
        var password = "Password123!";
        var request = new RegisterRequest(username, "loginuser@example.com", password);
        await _client.PostAsJsonAsync("/api/auth/register", request);
        var loginRequest = new LoginCommand(username, password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        tokenResponse.Should().NotBeNull();
        tokenResponse!.TokenType.Should().Be("Bearer");
        tokenResponse.AccessToken.Should().NotBeNullOrEmpty();
    }


    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenCredentialsAreInvalid()
    {
        // Arrange
        var loginRequest = new LoginRequest("non_existent_user", "WrongPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task ChangePassword_ShouldReturnUnauthorized_WhenNotLoggedIn()
    {
        // Arrange
        var request = new ChangePasswordRequest("OldPassword123!", "NewPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task ChangePassword_ShouldReturnOk_WhenLoggedInAndRequestIsValidAsync()
    {
        // Arrange
        var username = "change_pass_user";
        var email = "user@example.com";
        var oldPassword = "OldPassword123!";
        var newPassword = "NewPassword123!";
        var registerRequest = new RegisterRequest(username, email, oldPassword);
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(username, oldPassword);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var tokenData = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();

        var authClient = _factory.CreateClient();
        authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenData!.AccessToken);

        var changePassRequest = new ChangePasswordRequest(oldPassword, newPassword);
        // Act
        var response = await authClient.PostAsJsonAsync("/api/auth/change-password", changePassRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var oldLoginRequest = new LoginRequest(username, oldPassword);
        var oldLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", oldLoginRequest);
        oldLoginResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task ForgotPassword_ShouldReturnOk_RegardlessOfEmailExistence()
    {
        // Arrange
        var request = new ForgotPasswordRequest("any_email@example.com", "http://localhost/reset");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private class TokenResponse
    {
        public string TokenType { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
    }


}
