using Newtonsoft.Json;

namespace RAIT.Example.API.Endpoints.Endpoints.ActionResults;

public record ResponseDto(
    [property: JsonProperty("accountId")] string ExternalAccountId,
    [property: JsonProperty("ValueStr")] string ValueStr);