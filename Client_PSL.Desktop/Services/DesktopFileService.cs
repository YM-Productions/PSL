using System;
using System.IO;
using Client_PSL.Services;

namespace Client_PSL.Desktop.Services;

public class DesktopFileService : IFileService
{
    private readonly string _baseDir;
    private readonly string suffix = ".psl";

    public DesktopFileService(string appName = "PSL")
    {
        string basePath = Environment.GetFolderPath(
                OperatingSystem.IsWindows() ? Environment.SpecialFolder.ApplicationData :
                                              Environment.SpecialFolder.UserProfile);

        _baseDir = OperatingSystem.IsLinux()
            ? Path.Combine(basePath, ".config", appName)
            : Path.Combine(basePath, appName);

        Directory.CreateDirectory(_baseDir);
    }

    public string GetFilePath(string fileName)
    {
        if (!fileName.EndsWith(suffix))
            fileName += suffix;

        return Path.Combine(_baseDir, fileName);
    }

    public string? ReadText(string fileName)
    {
        if (!fileName.EndsWith(suffix))
            fileName += suffix;
        string path = GetFilePath(fileName);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    public void WriteText(string fileName, string content)
    {
        if (!fileName.EndsWith(suffix))
            fileName += suffix;
        string path = GetFilePath(fileName);
        File.WriteAllText(path, content);
    }

    public bool Exists(string fileName)
    {
        if (!fileName.EndsWith(suffix))
            fileName += suffix;
        string path = GetFilePath(fileName);
        return File.Exists(path);
    }
}
