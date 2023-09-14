using CloudinaryDotNet;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Umbraco.Extensions;
using Umbraco.StorageProviders.AzureBlob.IO;

namespace Umbraco.StorageProviders.AzureBlob;

public sealed class CloudinaryFileProvider : IFileProvider
{
    private readonly Cloudinary _containerClient;

    public CloudinaryFileProvider(Cloudinary containerClient)
    {
        _containerClient = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
    }

    public CloudinaryFileProvider(CloudinaryFileSystemOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _containerClient = new Cloudinary(new Account(options.Cloud, options.ApiKey, options.ApiSecret));
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        var path = GetFullPath(subpath);

        //// Get all blobs and iterate to fetch all pages
        //var blobs = _containerClient.GetBlobsByHierarchy(delimiter: "/", prefix: path).ToList();
        var directory = _containerClient.RootFolders().Folders.First();
        var files = _containerClient.ListResourcesByAssetFolder(directory.Path).Resources;

        return files.Length == 0
            ? NotFoundDirectoryContents.Singleton
            : new CloudinaryDirectoryContents(directory, files);
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var path = GetFullPath(subpath);
        //BlobClient blobClient = _containerClient.GetBlobClient(path);

        //BlobProperties properties;
        //try
        //{
        //    properties = blobClient.GetProperties().Value;
        //}
        //catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        //{
        //    return new NotFoundFileInfo(CloudinaryItemInfo.ParseName(path));
        //}

        var resource = _containerClient.GetResource(path);
        if (resource == null)
        {
            return new NotFoundFileInfo(CloudinaryItemInfo.ParseName(path));
        }

        return new CloudinaryItemInfo(resource);
    }

    public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

    private string GetFullPath(string subpath) => subpath.EnsureStartsWith('/');
}
