using System.ComponentModel.DataAnnotations;

namespace Umbraco.StorageProviders;

public sealed class CdnMediaUrlProviderOptions
{
    [Required]
    public Uri Url { get; set; } = null!;

    public bool RemoveMediaFromPath { get; set; } = true;
}
