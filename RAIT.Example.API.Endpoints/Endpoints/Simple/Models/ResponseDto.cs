using Newtonsoft.Json;

namespace RAIT.Example.API.Endpoints.Endpoints.Simple.Models;

public record ResponseDto(
    [property: JsonProperty("accountId")] string ExternalAccountId,
    [property: JsonProperty("ValueStr")] string ValueStr);