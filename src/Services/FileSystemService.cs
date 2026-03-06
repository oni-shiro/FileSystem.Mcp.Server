namespace FileSystem.Mcp.Server.Services;

public class FileSystemService : IFileSystemService
{
    private readonly string _rootDirectory;

    public string RootDirectory => _rootDirectory;

    public FileSystemService(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("Root directory cannot be null or empty.", nameof(rootDirectory));

        if (!Directory.Exists(rootDirectory))
            throw new DirectoryNotFoundException($"The root directory '{rootDirectory}' does not exist.");

        // Normalize the root directory path
        _rootDirectory = Path.GetFullPath(rootDirectory);
    }

    /// <summary>
    /// Validates that the given path is within the root directory (prevents directory traversal attacks).
    /// Returns the normalized full path if valid.
    /// </summary>
    private string ValidateAndNormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        // Resolve the full path
        string fullPath = Path.GetFullPath(path, _rootDirectory);

        // Check if the resolved path is within the root directory
        if (!fullPath.StartsWith(_rootDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
            fullPath != _rootDirectory)
        {
            throw new UnauthorizedAccessException(
                $"Access denied. Path '{path}' resolves outside the allowed root directory '{_rootDirectory}'.");
        }

        return fullPath;
    }

    public string ReadFile(string path)
    {
        string validatedPath = ValidateAndNormalizePath(path);

        if (!File.Exists(validatedPath))
            throw new FileNotFoundException($"The file at path '{path}' was not found.");

        return File.ReadAllText(validatedPath);
    }

    public void WriteFile(string path, string content)
    {
        string validatedPath = ValidateAndNormalizePath(path);

        // Ensure the directory exists
        string? directory = Path.GetDirectoryName(validatedPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(validatedPath, content);
    }

    public void CreateDirectory(string path)
    {
        string validatedPath = ValidateAndNormalizePath(path);

        if (!Directory.Exists(validatedPath))
            Directory.CreateDirectory(validatedPath);
    }

    public void DeleteDirectory(string path)
    {
        string validatedPath = ValidateAndNormalizePath(path);

        if (Directory.Exists(validatedPath))
            Directory.Delete(validatedPath, recursive: true);
    }

    public void DeleteFile(string path)
    {
        string validatedPath = ValidateAndNormalizePath(path);

        if (File.Exists(validatedPath))
            File.Delete(validatedPath);
    }

    public void CopyFile(string sourcePath, string destinationPath)
    {
        string validatedSourcePath = ValidateAndNormalizePath(sourcePath);
        string validatedDestPath = ValidateAndNormalizePath(destinationPath);

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
        string validatedSourcePath = ValidateAndNormalizePath(sourcePath);
        string validatedDestPath = ValidateAndNormalizePath(destinationPath);

        if (!File.Exists(validatedSourcePath))
            throw new FileNotFoundException($"Source file '{sourcePath}' was not found.");

        // Ensure destination directory exists
        string? destDirectory = Path.GetDirectoryName(validatedDestPath);
        if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
            Directory.CreateDirectory(destDirectory);

        File.Move(validatedSourcePath, validatedDestPath, overwrite: true);
    }

    /// <summary>
    /// Recursively loads the entire directory structure starting from the given path.
    /// Returns a dictionary with relative paths and metadata.
    /// </summary>
    public Dictionary<string, object> LoadDirectoryStructure(string path = "")
    {
        string basePath = string.IsNullOrEmpty(path) ? _rootDirectory : ValidateAndNormalizePath(path);

        if (!Directory.Exists(basePath))
            throw new DirectoryNotFoundException($"The directory at path '{path}' was not found.");

        var structure = new Dictionary<string, object>
        {
            { "path", GetRelativePath(basePath) },
            { "type", "directory" },
            { "children", new List<Dictionary<string, object>>() }
        };

        var children = (List<Dictionary<string, object>>)structure["children"];

        try
        {
            // Add directories
            foreach (var dir in Directory.GetDirectories(basePath))
            {
                children.Add(new Dictionary<string, object>
                {
                    { "path", GetRelativePath(dir) },
                    { "type", "directory" },
                    { "children", LoadDirectoryStructureRecursive(dir) }
                });
            }

            // Add files
            foreach (var file in Directory.GetFiles(basePath))
            {
                var fileInfo = new FileInfo(file);
                children.Add(new Dictionary<string, object>
                {
                    { "path", GetRelativePath(file) },
                    { "type", "file" },
                    { "size", fileInfo.Length }
                });
            }
        }
        catch (UnauthorizedAccessException)
        {
            // If we can't access certain directories, continue with what we can access
        }

        return structure;
    }

    private List<Dictionary<string, object>> LoadDirectoryStructureRecursive(string dirPath)
    {
        var children = new List<Dictionary<string, object>>();

        try
        {
            // Add subdirectories
            foreach (var dir in Directory.GetDirectories(dirPath))
            {
                children.Add(new Dictionary<string, object>
                {
                    { "path", GetRelativePath(dir) },
                    { "type", "directory" },
                    { "children", LoadDirectoryStructureRecursive(dir) }
                });
            }

            // Add files
            foreach (var file in Directory.GetFiles(dirPath))
            {
                var fileInfo = new FileInfo(file);
                children.Add(new Dictionary<string, object>
                {
                    { "path", GetRelativePath(file) },
                    { "type", "file" },
                    { "size", fileInfo.Length }
                });
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Continue if we can't access certain directories
        }

        return children;
    }

    /// <summary>
    /// Gets the relative path from the root directory.
    /// </summary>
    private string GetRelativePath(string fullPath)
    {
        if (fullPath == _rootDirectory)
            return ".";

        return Path.GetRelativePath(_rootDirectory, fullPath);
    }

    /// <summary>
    /// Validates that a path exists and is accessible as a potential root directory.
    /// Returns validation result with details.
    /// </summary>
    public Dictionary<string, object> ValidateRootDirectoryChange(string proposedPath)
    {
        var result = new Dictionary<string, object>
        {
            { "currentRootDirectory", _rootDirectory },
            { "proposedRootDirectory", proposedPath },
            { "isValid", false },
            { "message", "" },
            { "instructions", "" }
        };

        try
        {
            // Normalize the path
            string normalizedPath = Path.GetFullPath(proposedPath);

            // Check if it exists
            if (!Directory.Exists(normalizedPath))
            {
                result["message"] = $"The proposed path '{proposedPath}' does not exist or is not accessible.";
                return result;
            }

            // Try to list contents to verify access
            _ = Directory.GetFileSystemEntries(normalizedPath);

            result["isValid"] = true;
            result["proposedRootDirectory"] = normalizedPath;
            result["message"] = $"Root directory change requested: {_rootDirectory} → {normalizedPath}";
            result["instructions"] = $"To apply this change, restart the MCP server with:\n" +
                $"  Windows (PowerShell): $env:MCP_ROOT_DIR = '{normalizedPath}' ; dotnet run\n" +
                $"  Windows (CMD): set MCP_ROOT_DIR={normalizedPath} && dotnet run\n" +
                $"  Unix/Mac: export MCP_ROOT_DIR='{normalizedPath}' && dotnet run";

            return result;
        }
        catch (Exception ex)
        {
            result["message"] = $"Error validating path: {ex.Message}";
            return result;
        }
    }
}