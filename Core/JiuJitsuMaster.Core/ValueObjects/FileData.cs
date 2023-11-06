using CommonUtility.Model;

namespace JiuJitsuMaster.Core.ValueObjects;

public class FileData : ValueObject
{
    public FileData(byte[] content, string fileName, string contentType)
    {
        Content = content;
        FileName = fileName;
        ContentType = contentType;
    }

    public static FileData Create(byte[] content, string fileName, string contentType)
    {
        return new FileData(content, fileName, contentType);
    }

    public byte[] Content { get; }
    public string FileName { get; }
    public string ContentType { get; }


    protected override IEnumerable<object> GetEqualityComponents()
    {
        foreach (var byteComponent in Content)
            yield return byteComponent;

        yield return FileName;
        yield return ContentType;
    }
}