using System.Security.Cryptography;
using System.Text;
using Legend2Toolbox.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Legend2Toolbox.Api.IntegrationTests.Services;

public class LocalFileStorageServiceTests
{
    [Fact]
    public async Task SaveFileAsync_ShouldCalculateSha256AndPreserveContent()
    {
        var webRootPath = Path.Combine(Path.GetTempPath(), $"legend2toolbox-{Guid.NewGuid():N}");
        Directory.CreateDirectory(webRootPath);

        try
        {
            var environment = new TestWebHostEnvironment(webRootPath);
            var service = new LocalFileStorageService(environment);
            var fileBytes = Encoding.UTF8.GetBytes("material file storage test");
            var expectedSha256 = Convert.ToHexString(SHA256.HashData(fileBytes)).ToLowerInvariant();
            await using var source = new MemoryStream(fileBytes);
            var formFile = new FormFile(source, 0, source.Length, "file", "material.bin");

            var storedFile = await service.SaveFileAsync(formFile, "materials/test");

            storedFile.FileSize.Should().Be(fileBytes.Length);
            storedFile.Sha256.Should().Be(expectedSha256);

            await using var savedContent = await service.OpenReadAsync(storedFile.StoragePath);
            using var copiedContent = new MemoryStream();
            await savedContent.CopyToAsync(copiedContent);
            copiedContent.ToArray().Should().Equal(fileBytes);
        }
        finally
        {
            Directory.Delete(webRootPath, recursive: true);
        }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment(string webRootPath)
        {
            WebRootPath = webRootPath;
            ContentRootPath = webRootPath;
        }

        public string ApplicationName { get; set; } = "Legend2Toolbox.Api.IntegrationTests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Test";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; }
    }
}
