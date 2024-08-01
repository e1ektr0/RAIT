using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace RAIT.Example.API.Models;

public class ResponseModel
{
    public long Id { get; set; }
}

public record AttributeResponseModel
{
    [JsonPropertyName("test2")]
    [JsonProperty("test")]
    public string? Id { get; set; }
}