namespace Client_PSL.Services;

public interface IFileService
{
    string GetFilePath(string fileName);

    string? ReadText(string fileName);
    void WriteText(string fileName, string contnent);
    bool Exists(string fileName);
}
