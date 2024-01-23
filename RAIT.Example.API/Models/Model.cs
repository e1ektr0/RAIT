namespace RAIT.Example.API.Models;

public class Model
{
    public long Id { get; set; }
    public List<Guid>? List { get; set; }
}

public class ModelWithNullValues
{
    public long Id { get; set; }
    public string? NullValue { get; set; }
}

public class ModelWithFile
{
    public long Id { get; set; }
    public IFormFile File { get; set; } = null!;
}