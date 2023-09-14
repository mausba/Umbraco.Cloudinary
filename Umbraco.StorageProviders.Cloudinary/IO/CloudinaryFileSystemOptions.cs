using System.ComponentModel.DataAnnotations;

namespace Umbraco.StorageProviders.AzureBlob.IO;

public sealed class CloudinaryFileSystemOptions
{
    [Required]
    public string Cloud { get; set; } = null!;

    [Required]
    public string ApiKey { get; set; } = null!;

    [Required]
    public string ApiSecret { get; set; } = null!;

    [Required]
    public string VirtualPath { get; set; } = null!;
}
