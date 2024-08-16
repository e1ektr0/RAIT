using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RAIT.Example.API.Endpoints.Endpoints.FromQuery.Models;

public class CompanyModel
{
    public required string Test { get; set; }
    public required string Test2 { get; set; }
}

public record OperationResponse<TOrigin>(
    [property: JsonProperty("origin")] TOrigin? Origin,
    [property: JsonProperty("stale")] bool IsStale,
    [property: JsonProperty("meta_data")] OperationMetaData? MetaData);

public record OperationRequest<TOrigin>(
    [property: JsonProperty("origin")] TOrigin Origin,
    [property: JsonProperty("timeout_milliseconds")]
    long TimeoutMilliseconds,
    [property: JsonProperty("use_synchronous_execution")]
    bool? UseSynchronousExecution);

public record OperationMetaData(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("status")] OperationStatus Status,
    [property: JsonProperty("progress")] ProgressInfo Progress,
    [property: JsonProperty("started_time")]
    DateTime StartedTime,
    [property: JsonProperty("end_time")] DateTime? EndTime);

public enum OperationStatus
{
    [EnumMember(Value = "in_progress")] InProgress = 1,

    [EnumMember(Value = "completed")] Completed = 2,

    [EnumMember(Value = "failed")] Failed = 3,
}

public record ProgressInfo(
    [property: JsonProperty("current_step")]
    int CurrentStep,
    [property: JsonProperty("current_step_description")]
    string CurrentStepDescription,
    [property: JsonProperty("total_steps")]
    int TotalSteps);