namespace FileSystem.Mcp.Server.Services;

/// <summary>
/// Defines the interface for secure filesystem operations within a sandboxed root directory.
/// All paths are validated to prevent directory traversal attacks.
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// Reads the content of a file.
    /// </summary>
    string ReadFile(string path);

    /// <summary>
    /// Writes content to a file. Creates parent directories if needed.
    /// </summary>
    void WriteFile(string path, string content);

    /// <summary>
    /// Creates a directory at the specified path.
    /// </summary>
    void CreateDirectory(string path);

    /// <summary>
    /// Deletes a directory and all its contents.
    /// </summary>
    void DeleteDirectory(string path);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    void DeleteFile(string path);

    /// <summary>
    /// Copies a file from source to destination.
    /// </summary>
    void CopyFile(string sourcePath, string destinationPath);

    /// <summary>
    /// Moves a file from source to destination.
    /// </summary>
    void MoveFile(string sourcePath, string destinationPath);
}