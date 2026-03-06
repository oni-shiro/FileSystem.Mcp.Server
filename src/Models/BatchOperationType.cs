namespace FileSystem.Mcp.Server.Models;

/// <summary>
/// Defines the types of filesystem operations that can be executed in a batch.
/// </summary>
public enum BatchOperationType
{
    /// <summary>Read file content</summary>
    Read,
    
    /// <summary>Write content to a file</summary>
    Write,
    
    /// <summary>Create a directory</summary>
    CreateDirectory,
    
    /// <summary>Delete a file</summary>
    DeleteFile,
    
    /// <summary>Delete a directory and all its contents</summary>
    DeleteDirectory,
    
    /// <summary>Copy a file from source to destination</summary>
    CopyFile,
    
    /// <summary>Move a file from source to destination</summary>
    MoveFile,
    
    /// <summary>Get the directory structure</summary>
    GetDirectoryStructure,
    
    /// <summary>Check if a file exists</summary>
    FileExists,
    
    /// <summary>Check if a directory exists</summary>
    DirectoryExists
}
