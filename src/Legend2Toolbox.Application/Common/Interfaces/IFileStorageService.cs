
using Microsoft.AspNetCore.Http;

namespace Legend2Toolbox.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<(string StoragePath, long FileSize, string Sha256)> SaveFileAsync(
        IFormFile file,
        string subFolder,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default);

    Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default);
}
