using Microsoft.AspNetCore.Mvc;

namespace RAIT.Example.API.Endpoints.Endpoints.Simple.Models;

public class PostRequest
{
    [FromRoute(Name =nameof(ExternalAccountId))]
    public required string ExternalAccountId { get; set; }

    [FromBody]
    public required AggregatedGetRequest Origin { get; set; }
}