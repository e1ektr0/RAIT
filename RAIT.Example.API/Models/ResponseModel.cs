using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
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
    public string? Domain { get; set; }

    public string? ExternalId { get; set; }
}

public class InternalModelAttributes
{
    [FromRoute(Name = "ExternalAccountId")]
    public string ExternalAccountId { get; set; } = null!;

    [FromBody]
    public InternalModelInternalAttributes Model { get; set; }= null!;
}


public class InternalModelInternalAttributes
{
    public string Domain { get; set; } = null!;
}