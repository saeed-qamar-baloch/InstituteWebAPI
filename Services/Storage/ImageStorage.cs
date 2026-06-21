namespace InstituteWebAPI.Services.Storage;

/// <summary>
/// Resolves the single filesystem root used to save and serve all images.
/// Set Images:RootPath (Images__RootPath as an environment variable) to a
/// persistent directory in production. Relative paths are resolved from the
/// application's content root; an empty value keeps the existing Images folder.
/// </summary>
public sealed class ImageStorage
{
    public string RootPath { get; }

    public ImageStorage(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredRoot = configuration["Images:RootPath"];
        RootPath = string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.Combine(environment.ContentRootPath, "Images")
            : Path.GetFullPath(configuredRoot, environment.ContentRootPath);
    }

    public string GetFolder(string folder) => Path.Combine(RootPath, folder);
}
