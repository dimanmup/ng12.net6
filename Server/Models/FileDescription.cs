using Newtonsoft.Json;

namespace Server.Models;

public class FileDescription
{
    public readonly string Name;
    public readonly string Path;
    public readonly long? Size;
    public readonly DateTime? CreationTimeUtc;
    public readonly DateTime? LastWriteTimeUtc;
    public readonly string? Error;

    public FileDescription(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileName(Path);

        FileInfo fi = new FileInfo(path);

        if (!fi.Exists) 
        {
            Error = "File not found.";
        }

        CreationTimeUtc = fi.CreationTimeUtc;
        LastWriteTimeUtc = fi.CreationTimeUtc;
        Size = fi.Length;
    }

    public string GetJson()
    {
        return JsonConvert.SerializeObject(this, new JsonSerializerSettings
        {
            DateFormatString = "yyyy.MM.dd HH:mm:ss",
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        });
    }
}