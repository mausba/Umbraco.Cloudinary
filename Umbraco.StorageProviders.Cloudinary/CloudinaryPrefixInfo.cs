using Microsoft.Extensions.FileProviders;

namespace Umbraco.StorageProviders.AzureBlob;

public sealed class CloudinaryPrefixInfo : IFileInfo
{
    public CloudinaryPrefixInfo(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        Name = ParseName(prefix);
    }

    public bool Exists => true;

    public bool IsDirectory => true;

    public DateTimeOffset LastModified => default;

    public long Length => -1;

    public string Name { get; }

    public string PhysicalPath => null!;

    public Stream CreateReadStream() => throw new InvalidOperationException();

    private static string ParseName(string prefix)
    {
        var name = prefix.TrimEnd('/');

        return name[(name.LastIndexOf('/') + 1)..];
    }
}
