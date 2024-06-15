namespace RAIT.Example.API.Models;

public enum EnumExample
{
    One,
    Two,
    Three
}
public class Model
{
    public long Id { get; set; }
    public List<Guid>? List { get; set; }
    public List<EnumExample>? EnumList { get; set; }
    public string? Domain { get; set; }
    public string? ExtraField { get; set; }
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