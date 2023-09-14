using System.Collections;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.FileProviders;

namespace Umbraco.StorageProviders.AzureBlob;

public sealed class CloudinaryDirectoryContents : IDirectoryContents
{
    private readonly Folder _containerClient;
    private readonly IReadOnlyCollection<Resource> _items;

    public CloudinaryDirectoryContents(Folder containerClient, IReadOnlyCollection<Resource> items)
    {
        _containerClient = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
        _items = items ?? throw new ArgumentNullException(nameof(items));

        Exists = items.Count > 0;
    }

    public bool Exists { get; }

    public IEnumerator<IFileInfo> GetEnumerator()
        => _items.Select((Func<Resource, IFileInfo>)(x => new CloudinaryItemInfo(x))).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
