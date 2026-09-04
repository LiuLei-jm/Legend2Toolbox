
using Microsoft.AspNetCore.Http;

namespace Legend2Toolbox.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<(string StoragePath, long FileSize)> SaveFileAsync(IFormFile file, string subFolder, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default);
}
