using System.Security.Cryptography;
using System.Text;

namespace Legend2Toolbox.Api.IntegrationTests.Endpoints;

[Collection("Integration Tests")]
public class MaterialFileEndpointsTests : IClassFixture<SqliteWebApplicationFactory<Program>>
{
    private readonly SqliteWebApplicationFactory<Program> _factory;

    public MaterialFileEndpointsTests(SqliteWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UploadAndDownload_ShouldPersistSha256AndReturnOriginalBytes()
    {
        var client = await CreateAuthenticatedClientAsync();
        var scriptSetId = await CreateScriptSetAsync(client);
        var fileBytes = Encoding.UTF8.GetBytes("material file content for sha256 verification");
        var expectedSha256 = Convert.ToHexString(SHA256.HashData(fileBytes)).ToLowerInvariant();
        var materialFileId = Guid.Empty;

        try
        {
            materialFileId = await UploadMaterialFileAsync(client, scriptSetId, fileBytes);

            var materialFiles = await client.GetFromJsonAsync<List<MaterialFileResponse>>(
                $"/api/scripts/material/by-set/{scriptSetId}");
            var materialFile = materialFiles.Should().ContainSingle().Subject;
            materialFile.Id.Should().Be(materialFileId);
            materialFile.Sha256.Should().Be(expectedSha256);

            var response = await client.GetAsync($"/api/scripts/material/{materialFileId}/content");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
            response.Headers.GetValues("X-Checksum-SHA256").Should().ContainSingle(expectedSha256);
            (await response.Content.ReadAsByteArrayAsync()).Should().Equal(fileBytes);
        }
        finally
        {
            if (materialFileId != Guid.Empty)
            {
                await client.DeleteAsync($"/api/scripts/material/{materialFileId}");
            }

            await client.DeleteAsync($"/api/scripts/sets/{scriptSetId}");
        }
    }

    [Fact]
    public async Task Download_ShouldReturnNotFound_WhenMaterialBelongsToAnotherUser()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var otherClient = await CreateAuthenticatedClientAsync();
        var scriptSetId = await CreateScriptSetAsync(ownerClient);
        var materialFileId = Guid.Empty;

        try
        {
            materialFileId = await UploadMaterialFileAsync(
                ownerClient,
                scriptSetId,
                Encoding.UTF8.GetBytes("private material"));

            var response = await otherClient.GetAsync($"/api/scripts/material/{materialFileId}/content");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            if (materialFileId != Guid.Empty)
            {
                await ownerClient.DeleteAsync($"/api/scripts/material/{materialFileId}");
            }

            await ownerClient.DeleteAsync($"/api/scripts/sets/{scriptSetId}");
        }
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N");
        var username = $"material_{suffix}";
        var password = "Password123!";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { Username = username, Email = $"{username}@test.com", Password = password });
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { Username = username, Password = password });
        loginResponse.EnsureSuccessStatusCode();
        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    private static async Task<Guid> CreateScriptSetAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/scripts/sets",
            new { Name = $"material-test-{Guid.NewGuid():N}", Description = "integration test" });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task<Guid> UploadMaterialFileAsync(
        HttpClient client,
        Guid scriptSetId,
        byte[] fileBytes)
    {
        using var multipart = new MultipartFormDataContent();
        multipart.Add(new StringContent(scriptSetId.ToString()), "scriptSetId");
        multipart.Add(new StringContent("Data/material.bin"), "targetPath");
        multipart.Add(new StringContent(string.Empty), "password");

        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        multipart.Add(fileContent, "file", "material.bin");

        var response = await client.PostAsync("/api/scripts/material", multipart);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private sealed record MaterialFileResponse(
        Guid Id,
        string FileName,
        string TargetPath,
        string Password,
        long FileSize,
        string Sha256);

    private sealed record TokenResponse(string AccessToken);
}
