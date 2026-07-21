namespace Legend2Toolbox.Api.IntegrationTests.Endpoints;

[Collection("Integration Tests")]
public class AdminUserEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AdminUserEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }


    [Theory]
    [InlineData("GET", "/api/admin/users")]
    [InlineData("PUT", "/api/admin/users/123/lock")]
    [InlineData("POST", "/api/admin/users/123/roles")]
    [InlineData("PUT", "/api/admin/users/123")]
    [InlineData("DELETE", "/api/admin/users/123")]
    public async Task AdminEndpoints_ShouldRetureUnauthorized_WhenNotLoggedIn(string method, string url)
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), url);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }



    [Theory]
    [InlineData("GET", "/api/admin/users")]
    [InlineData("PUT", "/api/admin/users/123/lock")]
    [InlineData("POST", "/api/admin/users/123/roles")]
    [InlineData("PUT", "/api/admin/users/123")]
    [InlineData("DELETE", "/api/admin/users/123")]
    public async Task AdminEndpoints_ShouldReturnForbidden_WhenUserIsNotSuperAdmin(string method, string url)
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("normal_hacker", Roles.Member.ToString());
        var request = new HttpRequestMessage(new HttpMethod(method), url);

        if (method == "PUT" || method == "POST") request.Content = JsonContent.Create(new { });

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }


    [Fact]
    public async Task GetUsers_ShouldReturnPagedList_WhenUserIsSuperAdmin()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("super_admin_get", Roles.SuperAdmin.ToString());

        // Act
        var response = await client.GetAsync("/api/admin/users?pageNumber=1&pageSize5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("pageNumber");
        content.Should().Contain("totalCount");
    }



    [Fact]
    public async Task UpdateUser_ShouldReturnOk_WhenUserIsSuperAdmin()
    {
        // Arrange
        var adminClient = await CreateAuthenticatedClientAsync("super_admin_update", Roles.SuperAdmin.ToString());

        var targetUserId = await SeedUserAsync("target_user", "target@test.com");
        var updateRequest = new { Username = "updated_name", Email = "updated@test.com" };
        // Act
        var response = await adminClient.PutAsJsonAsync($"/api/admin/users/{targetUserId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<string> SeedUserAsync(string username, string email)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser { UserName = username, Email = email };
        await userManager.CreateAsync(user, "Password123!");
        return user.Id.ToString();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string username, string role)
    {
        var client = _factory.CreateClient();
        var email = $"{username}@test.com";
        var password = "Password123!";

        await client.PostAsJsonAsync("/api/auth/register", new { Username = username, Email = email, Password = password });

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = role });
                }
                await userManager.AddToRoleAsync(user, role);
            }
        }

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { Username = username, Password = password });
        var tokenData = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenData!.AccessToken);
        return client;
    }

    private class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
