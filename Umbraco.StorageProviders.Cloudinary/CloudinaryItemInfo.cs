using CloudinaryDotNet.Actions;
using Microsoft.Extensions.FileProviders;

namespace Umbraco.StorageProviders.AzureBlob;

public sealed class CloudinaryItemInfo : IFileInfo
{
    private readonly Resource _blobClient;

    public CloudinaryItemInfo(Resource blobClient)
    {
        _blobClient = blobClient ?? throw new ArgumentNullException(nameof(blobClient));

        Name = ParseName(blobClient.DisplayName);
    }

    public CloudinaryItemInfo(GetResourceResult result)
    {
        Name = ParseName(result.DisplayName);
    }

    public bool Exists => true;

    public bool IsDirectory => false;

    public DateTimeOffset LastModified { get; }

    public long Length { get; }

    public string Name { get; }

    public string PhysicalPath => null!;

    public Stream CreateReadStream() => null;

    internal static string ParseName(string path) => path[(path.LastIndexOf('/') + 1)..];
}
