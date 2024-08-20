using Microsoft.AspNetCore.Mvc;

namespace RAIT.Example.API.Endpoints.Endpoints.Simple.Models;
public class AliasAttribute : FromQueryAttribute
{
    public AliasAttribute(string alias):base()
    {
        Name = alias;
    }
}
public class PostRequest
{
    [FromRoute(Name =nameof(ExternalAccountId))]
    public required string ExternalAccountId { get; set; }

    [FromBody]
    public required AggregatedGetRequest Origin { get; set; }
    
    [Alias("Field")]
    public string? Test { get; set; }
}