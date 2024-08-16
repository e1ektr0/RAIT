using Microsoft.AspNetCore.Mvc;

namespace RAIT.Example.API.Endpoints.Endpoints.Simple.Models;

public class AggregatedGetRequest
{
    [FromRoute(Name = nameof(ExternalAccountId))]
    public required string ExternalAccountId { get; set; }

    [FromQuery(Name = nameof(ValueStr))]
    public required string ValueStr { get; set; }
    [FromQuery] public FromInternalModel? Model { get; set; }
    public DateOnly Date { get; set; }
}

public class FromInternalModel
{
    public string? Test { get; set; }
}