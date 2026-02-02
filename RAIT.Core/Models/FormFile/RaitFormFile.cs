using Microsoft.AspNetCore.Http;

namespace RAIT.Core;

public class RaitDocIgnoreAttribute : Attribute;
public class RaitFormFile : IFormFile, IDisposable
{
    private Stream? _openReadStream;
    private readonly byte[]? _content;

    public RaitFormFile(string name, string contentType, byte[]? content = null)
    {
        Name = name;
        FileName = name;
        ContentType = contentType;
        _content = content;
    }

    public Stream OpenReadStream()
    {
        if (_content != null)
        {
            _openReadStream = new MemoryStream(_content);
        }
        else
            _openReadStream = File.Open(Name, FileMode.Open);

        return _openReadStream;
    }

    public void CopyTo(Stream target)
    {
        throw new NotImplementedException();
    }

    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    public string ContentType { get; }
    public string? ContentDisposition { get; } = null;
    [RaitDocIgnore]
    public IHeaderDictionary Headers { get; } = new HeaderDictionary();
    public long Length { get; } = 0;
    public string Name { get; }
    public string FileName { get; }

    public void Dispose()
    {
        if (_openReadStream == null) return;
        _openReadStream.Close();
        _openReadStream = null;
    }
}