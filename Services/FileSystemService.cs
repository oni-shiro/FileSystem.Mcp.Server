namespace FileSystem.Mcp.Server.Services;

public static class FileSystemService
{
    public static string ReadFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The file at path '{path}' was not found.");
        }
        return File.ReadAllText(path);
    }

    public static void WriteFile(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    public static List<string> ListDirectoriesAndFiles(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"The directory at path '{path}' was not found.");
        }

        var entries = new List<string>();
        // include full paths for both directories and files
        entries.AddRange(Directory.GetDirectories(path).Select(dir => dir));
        entries.AddRange(Directory.GetFiles(path).Select(file => file));
        return entries;
    }
}