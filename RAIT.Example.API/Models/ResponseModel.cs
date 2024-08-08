using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace RAIT.Example.API.Models;

public class ResponseModel
{
    public long Id { get; set; }
}

public class AttributeResponseModel
{
    [JsonPropertyName("test")]
    [JsonProperty("test")]
    public string? Id { get; set; }
}