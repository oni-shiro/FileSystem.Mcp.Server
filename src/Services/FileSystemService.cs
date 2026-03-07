namespace FileSystem.Mcp.Server.Services;

internal class FileSystemService : IFileSystemService
{
    private readonly RootProvider _rootProvider;

    public FileSystemService(RootProvider rootProvider)
    {
        _rootProvider = rootProvider;
        Console.WriteLine($"FileSystemService initialized with root directory: {_rootProvider.RootPath}");
    }

    public string ReadFile(string path)
    {
        string validatedPath = _rootProvider.Resolve(path);

        if (!File.Exists(validatedPath))
            throw new FileNotFoundException($"The file at path '{path}' was not found.");

        return File.ReadAllText(validatedPath);
    }

    public void WriteFile(string path, string content)
    {
        string validatedPath = _rootProvider.Resolve(path);

        // Ensure the directory exists
        string? directory = Path.GetDirectoryName(validatedPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(validatedPath, content);
    }

    public void CreateDirectory(string path)
    {
        string validatedPath = _rootProvider.Resolve(path);

        if (!Directory.Exists(validatedPath))
            Directory.CreateDirectory(validatedPath);
    }

    public void DeleteDirectory(string path)
    {
        string validatedPath = _rootProvider.Resolve(path);

        if (Directory.Exists(validatedPath))
            Directory.Delete(validatedPath, recursive: true);
    }

    public void DeleteFile(string path)
    {
        string validatedPath = _rootProvider.Resolve(path);

        if (File.Exists(validatedPath))
            File.Delete(validatedPath);
    }

    public void CopyFile(string sourcePath, string destinationPath)
    {
        string validatedSourcePath = _rootProvider.Resolve(sourcePath);
        string validatedDestPath = _rootProvider.Resolve(destinationPath);

        if (!File.Exists(validatedSourcePath))
            throw new FileNotFoundException($"Source file '{sourcePath}' was not found.");

        // Ensure destination directory exists
        string? destDirectory = Path.GetDirectoryName(validatedDestPath);
        if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
            Directory.CreateDirectory(destDirectory);

        File.Copy(validatedSourcePath, validatedDestPath, overwrite: true);
    }

    public void MoveFile(string sourcePath, string destinationPath)
    {
        string validatedSourcePath = _rootProvider.Resolve(sourcePath);
        string validatedDestPath = _rootProvider.Resolve(destinationPath);

        if (!File.Exists(validatedSourcePath))
            throw new FileNotFoundException($"Source file '{sourcePath}' was not found.");

        // Ensure destination directory exists
        string? destDirectory = Path.GetDirectoryName(validatedDestPath);
        if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
            Directory.CreateDirectory(destDirectory);

        File.Move(validatedSourcePath, validatedDestPath, overwrite: true);
    }
}