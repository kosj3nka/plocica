using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Plocica.Services;

public interface IBlobStorageService
{
    Task<string> UploadAsync(IFormFile file, CancellationToken ct = default);
    Task DeleteAsync(string? url, CancellationToken ct = default);
}

public class BlobStorageService : IBlobStorageService
{
    public const string ContainerName = "images";
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    private readonly BlobContainerClient _container;

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _container = blobServiceClient.GetBlobContainerClient(ContainerName);
    }

    public async Task<string> UploadAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("Datoteka je prazna.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException("Datoteka je prevelika (maksimalno 5 MB).");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension) || !AllowedContentTypes.Contains(file.ContentType))
        {
            throw new ArgumentException("Dozvoljeni su samo JPG, PNG i WEBP formati.");
        }

        var blobName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var blobClient = _container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType } },
            ct);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string? url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        var blobName = Path.GetFileName(new Uri(url).LocalPath);
        if (string.IsNullOrWhiteSpace(blobName))
        {
            return;
        }

        await _container.GetBlobClient(blobName).DeleteIfExistsAsync(cancellationToken: ct);
    }
}
