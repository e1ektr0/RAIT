using Newtonsoft.Json;

namespace RAIT.Example.API.Models;

public enum EnumExample
{
    One,
    Two,
    Three
}

public record AttributeModel(
    [property: JsonProperty("test")] string Id
);
public class Model
{
    public long Id { get; set; }
    public Guid? Guid { get; set; }
    public List<Guid>? List { get; set; }
    public List<EnumExample>? EnumList { get; set; }
    public string? Domain { get; set; }
    public string? ExtraField { get; set; }
    public ModelIncluded? ModelIncluded { get; set; }
    public List<ModelIncludeInList>? ModelIncludeInLists { get; set; }
}

public class ModelIncludeInList
{
    public string XSecret2 { get; set; }

}
public class ModelIncluded
{
    public string Secret { get; set; }
    public Uri? Uri { get; set; }
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