using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;

namespace Umbraco.StorageProviders.AzureBlob.IO;

public sealed class CloudinaryFileSystem : IFileSystem, IFileProviderFactory
{
    private readonly string _rootUrl;

    private readonly Cloudinary _container;
    private readonly IIOHelper _ioHelper;
    private readonly IContentTypeProvider _contentTypeProvider;

    public CloudinaryFileSystem(CloudinaryFileSystemOptions options, IHostingEnvironment hostingEnvironment, IIOHelper ioHelper, IContentTypeProvider contentTypeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(hostingEnvironment);

        _rootUrl = EnsureUrlSeparatorChar(hostingEnvironment.ToAbsolute(options.VirtualPath)).TrimEnd('/');
        _container = new Cloudinary(new Account(options.Cloud, options.ApiKey, options.ApiSecret));
        _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
        _contentTypeProvider = contentTypeProvider ?? throw new ArgumentNullException(nameof(contentTypeProvider));
    }

    public bool CanAddPhysical => false;

    public IEnumerable<string> GetDirectories(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return _container.RootFolders().Folders.Select(f => f.Path);
        //return ListResources(GetDirectoryPath(path))
        //    .Where(x => x.IsPrefix)
        //    .Select(x => GetRelativePath($"/{x.Prefix}").Trim('/'));
    }

    public void DeleteDirectory(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        DeleteDirectory(path, true);
    }

    public void DeleteDirectory(string path, bool recursive)
    {
        ArgumentNullException.ThrowIfNull(path);

        _container.DeleteFolder(path);

        //foreach (var blob in ListResources(GetDirectoryPath(path)))
        //{
        //    if (blob.IsPrefix)
        //    {
        //        DeleteDirectory(blob.Prefix, true);
        //    }
        //    else if (blob.IsBlob)
        //    {
        //        _container.GetBlobClient(blob.Blob.Name).DeleteIfExists();
        //    }
        //}
    }

    public bool DirectoryExists(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        var folder = _container.RootFolders(new GetFoldersParams
        {
            MaxResults = 500
        }).Folders.FirstOrDefault(f => f.Path == path);

        return folder != null;
    }

    public void AddFile(string path, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(stream);

        AddFile(path, stream, true);
    }

    public void AddFile(string path, Stream stream, bool overrideIfExists)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(stream);

        //var headers = new BlobHttpHeaders();
        //if (_contentTypeProvider.TryGetContentType(path, out var contentType))
        //{
        //    headers.ContentType = contentType;
        //}

        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(path, stream)
        };

        _ = _container.Upload(uploadParams);
    }

    public void AddFile(string path, string physicalPath, bool overrideIfExists = true, bool copy = false)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(physicalPath);

        //BlobClient destinationBlob = GetBlobClient(path);
        //if (!overrideIfExists && destinationBlob.Exists())
        //{
        //    throw new InvalidOperationException($"A file at path '{path}' already exists");
        //}

        //BlobClient sourceBlob = GetBlobClient(physicalPath);
        //BlobRequestConditions? destinationConditions = overrideIfExists ? null : new BlobRequestConditions
        //{
        //    IfNoneMatch = ETag.All
        //};

        //CopyFromUriOperation copyFromUriOperation = destinationBlob.StartCopyFromUri(sourceBlob.Uri, new BlobCopyFromUriOptions()
        //{
        //    DestinationConditions = destinationConditions
        //});

        //if (copyFromUriOperation?.HasCompleted == false)
        //{
        //    copyFromUriOperation.WaitForCompletion();
        //}

        //if (!copy)
        //{
        //    sourceBlob.DeleteIfExists();
        //}
    }

    public IEnumerable<string> GetFiles(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return GetFiles(path, null);
    }

    public IEnumerable<string> GetFiles(string path, string? filter)
    {
        ArgumentNullException.ThrowIfNull(path);

        var folders = _container.RootFolders(new GetFoldersParams
        {
            MaxResults = 500
        });

        return _container.ListResources().Resources.ToList().Select(r => r.DisplayName);

        //TODO: implement filter
        //var files = ListBlobs(GetDirectoryPath(path)).Where(x => x.IsBlob).Select(x => x.Blob.Name);
        //if (!string.IsNullOrEmpty(filter) && filter != "*.*")
        //{
        //    // TODO: Might be better to use a globbing library
        //    filter = filter.TrimStart("*");
        //    files = files.Where(x => x.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) > -1);
        //}

        //return files.Select(x => GetRelativePath($"/{x}"));
    }

    public Stream OpenFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var file = _container.GetResourceAsync(path).Result;
        return null;

        //return GetBlobClient(path).OpenRead();
    }

    public void DeleteFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        //GetBlobClient(path).DeleteIfExists();
    }

    public bool FileExists(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var file = _container.GetResourceAsync(path).Result;

        return file != null;
    }

    public string GetRelativePath(string fullPathOrUrl)
    {
        ArgumentNullException.ThrowIfNull(fullPathOrUrl);

        // test url
        var path = EnsureUrlSeparatorChar(fullPathOrUrl); // ensure url separator char

        // if it starts with the root url, strip it and trim the starting slash to make it relative
        // eg "/Media/1234/img.jpg" => "1234/img.jpg"
        if (_ioHelper.PathStartsWith(path, _rootUrl, '/'))
        {
            path = path[_rootUrl.Length..].TrimStart('/');
        }

        // unchanged - what else?
        return path;
    }

    public string GetFullPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        path = EnsureUrlSeparatorChar(path);
        return (_ioHelper.PathStartsWith(path, _rootUrl, '/') ? path : $"{_rootUrl}/{path}").Trim('/');
    }

    public string GetUrl(string? path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return $"{_rootUrl}/{EnsureUrlSeparatorChar(path).Trim('/')}";
    }

    public DateTimeOffset GetLastModified(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return DateTime.Now;
        //return GetBlobClient(path).GetProperties().Value.LastModified;
    }

    public DateTimeOffset GetCreated(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return DateTime.Now;
        //return GetBlobClient(path).GetProperties().Value.CreatedOn;
    }

    public long GetSize(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return 220;
        //return GetBlobClient(path).GetProperties().Value.ContentLength;
    }

    public IFileProvider Create() => new CloudinaryFileProvider(_container);

    private static string EnsureUrlSeparatorChar(string path)
        => path.Replace("\\", "/", StringComparison.InvariantCultureIgnoreCase);

    private string GetDirectoryPath(string fullPathOrUrl)
    {
        var path = GetFullPath(fullPathOrUrl);

        return path.Length == 0 ? path : path.EnsureEndsWith('/');
    }

    private async Task<IEnumerable<Resource>> ListResources(string path)
        => (await _container.ListResourceByAssetFolderAsync(path, false, false, false)).Resources;

    private string GetBlobPath(string path)
    {
        path = EnsureUrlSeparatorChar(path);

        //if (_ioHelper.PathStartsWith(path, _containerRootPath, '/'))
        //{
        //    return path;
        //}

        //if (_ioHelper.PathStartsWith(path, _rootUrl, '/'))
        //{
        //    path = path[_rootUrl.Length..];
        //}

        //path = $"{_containerRootPath}/{path.TrimStart('/')}";

        return path.Trim('/');
    }
}
