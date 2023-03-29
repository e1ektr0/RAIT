using Microsoft.AspNetCore.Http;

namespace RAIT.Core;

public class RaitFormFile : IFormFile, IDisposable
{
    private FileStream? _openReadStream;

    public RaitFormFile(string name, string contentType)
    {
        Name = name;
        FileName = name;
        ContentType = contentType;
    }

    public Stream OpenReadStream()
    {
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