using FileSystem.Mcp.Server.Models;

namespace FileSystem.Mcp.Server.Services;

internal class FileSystemService : IFileSystemService
{
    private readonly RootProvider _rootProvider;
    private readonly IFileReader _fileReader;

    public FileSystemService(RootProvider rootProvider, IFileReader fileReader)
    {
        _rootProvider = rootProvider;
        _fileReader = fileReader;
    }

    public string ReadFile(string path)
    {
        string validatedPath = _rootProvider.Resolve(path);
        if (!File.Exists(validatedPath))
            throw new FileNotFoundException($"The file at path '{path}' was not found.");

        string extension = Path.GetExtension(validatedPath).ToLowerInvariant();
        if (extension == ".pdf")
        {
            return _fileReader.ReadPdf(validatedPath);
        }

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

    public DirectoryNode BuildDirectoryMap(string path, int depth = 10)
    {
        DirectoryInfo rootDirInfo = new DirectoryInfo(_rootProvider.Resolve(path));
        DirectoryNode rootNode = new DirectoryNode
        {
            Path = rootDirInfo.FullName,
            Children = new List<DirectoryNode>(),
            Files = new List<FileNode>(),
            LastModified = rootDirInfo.LastWriteTimeUtc
        };

        foreach (var file in rootDirInfo.GetFiles())
        {
            rootNode.Files.Add(new FileNode
            {
                Name = file.Name,
                RelativePath = Path.GetRelativePath(_rootProvider.RootPath, file.FullName),
                Size = file.Length,
                Extension = file.Extension,
                LastModified = file.LastWriteTimeUtc,
                IsReadonly = file.IsReadOnly
            });
        }

        foreach(var dir in rootDirInfo.GetDirectories())
        {
            if (depth > 0)
            {
                rootNode.Children.Add(BuildDirectoryMap(Path.GetRelativePath(_rootProvider.RootPath, dir.FullName), depth - 1));
            }
        }
        
        return rootNode;
    }
}