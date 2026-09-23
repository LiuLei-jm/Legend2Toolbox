
using Microsoft.AspNetCore.Hosting;

namespace Legend2Toolbox.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private const string UploadsFolderName = "uploads";
    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<(string StoragePath, long FileSize, string Sha256)> SaveFileAsync(
        IFormFile file,
        string subFolder,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("上传的文件不能为空", nameof(file));
        }
        var rootPath = GetRootPath();
        var targetDirectory = Path.Combine(rootPath, UploadsFolderName, subFolder);

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        var extension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(targetDirectory, uniqueFileName);

        using var sha256 = SHA256.Create();
        try
        {
            await using var stream = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);
            await using var hashingStream = new CryptoStream(
                stream,
                sha256,
                CryptoStreamMode.Write,
                leaveOpen: false);

            await file.CopyToAsync(hashingStream, cancellationToken);
            await hashingStream.FlushFinalBlockAsync(cancellationToken);
        }
        catch
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
            throw;
        }

        var relativePath = Path.Combine(UploadsFolderName, subFolder, uniqueFileName);
        var hash = Convert.ToHexString(sha256.Hash!).ToLowerInvariant();

        return (relativePath, new FileInfo(fullPath).Length, hash);
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = ResolveStoragePath(storagePath);

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        return Task.FromResult(stream);
    }

    public Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return Task.CompletedTask;
        }
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = ResolveStoragePath(storagePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetRootPath()
    {
        return string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;
    }

    private string ResolveStoragePath(string storagePath)
    {
        var rootPath = Path.GetFullPath(GetRootPath());
        var sanitizedStoragePath = storagePath.TrimStart('/', '\\');
        var fullPath = Path.GetFullPath(Path.Combine(rootPath, sanitizedStoragePath));
        var rootPrefix = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                         + Path.DirectorySeparatorChar;
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (!fullPath.StartsWith(rootPrefix, comparison))
        {
            throw new InvalidOperationException("文件存储路径超出了允许的根目录");
        }

        return fullPath;
    }
}
