using Legend2Toolbox.Domain.Common;

namespace Legend2Toolbox.Domain.Entities.ScriptSets;

public class MaterialFile : BaseEntity
{
    public Guid ScriptSetId { get; private set; }

    public string FileName { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public string TargetPath { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public string Sha256 { get; private set; } = string.Empty;

    public long FileSize { get; private set; }

    public ScriptSet? ScriptSet { get; set; }
    private MaterialFile()
    {

    }
    public static MaterialFile Create(
        Guid scriptSetId,
        string fileName,
        string storagePath,
        string targetPath,
        string password,
        long fileSize,
        string sha256)
    {
        return new MaterialFile
        {
            ScriptSetId = scriptSetId,
            FileName = fileName,
            StoragePath = storagePath,
            TargetPath = targetPath,
            Password = password,
            FileSize = fileSize,
            Sha256 = sha256
        };
    }

    public void Update(
        string fileName,
        string storagePath,
        string targetPath,
        string password,
        long fileSize,
        string sha256)
    {
        FileName = fileName;
        StoragePath = storagePath;
        TargetPath = targetPath;
        Password = password;
        FileSize = fileSize;
        Sha256 = sha256;
    }

    public void SetSha256(string sha256)
    {
        Sha256 = sha256;
    }
}
