using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;
using RAIT.Example.API.Test.Infrastructure;

namespace RAIT.Example.API.Test;

public sealed class RaitTypedResultsTests : RaitConfiguredTestBase
{
    [Test]
    public async Task GetAsyncResults_ReturnsOk()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.GetAsyncResults(1));
        var okResult = (Ok<WeatherForecast>)response!.Result;

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Result, Is.TypeOf<Ok<WeatherForecast>>());
        Assert.That(okResult.Value, Is.Not.Null);
        Assert.That(okResult.Value!.Id, Is.EqualTo(20));
    }

    [Test]
    public async Task GetAsyncResults_ReturnsNotFound_WithPayload()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.GetAsyncResults(0));

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Result, Is.TypeOf<NotFound<string>>());
        Assert.That(((NotFound<string>)response.Result).Value, Is.EqualTo("Weather forecast '0' was not found."));
    }

    [Test]
    public async Task GetAsyncResultsValidationProblem_ReturnsValidationProblem()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.GetAsyncResultsValidationProblem(0));

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Result, Is.TypeOf<ValidationProblem>());

        var validationProblem = (ValidationProblem)response.Result;
        Assert.That(validationProblem.ProblemDetails.Status, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(validationProblem.ProblemDetails.Errors, Contains.Key("id"));
        Assert.That(validationProblem.ProblemDetails.Errors["id"], Is.EqualTo(new[] { "Id must be greater than zero." }));
    }

    [Test]
    public async Task GetAsyncResultsWithoutBody_ReturnsNotFound()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.GetAsyncResultsWithoutBody(0));

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Result, Is.AssignableTo<IStatusCodeHttpResult>());
        Assert.That(((IStatusCodeHttpResult)response.Result).StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
        Assert.That(response.Result, Is.TypeOf<NotFound>());
    }

    [Test]
    public async Task GetCreated_ReturnsCreated()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallHttpAsync(c => c.GetCreated(1));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task GetAccepted_ReturnsAccepted()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallRequiredAsync(c => c.GetAccepted());

        Assert.That(response.Value!.Id, Is.EqualTo(42));
    }

    [Test]
    public async Task Delete_ReturnsNoContent_WhenExists()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.Delete(5));

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Result, Is.TypeOf<NoContent>());
    }

    [Test]
    public async Task Delete_ReturnsNoContent_WhenExists_WithRequiredCall()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallRequiredAsync(c => c.Delete(5));

        Assert.That(response.Result, Is.TypeOf<NoContent>());
    }

    [Test]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.Delete(0));

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Result, Is.TypeOf<NotFound>());
    }

    [Test]
    public async Task CreateBadRequest_Returns400()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.CreateBadRequest());

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Value, Is.EqualTo("Invalid payload or business rule violation"));
        Assert.That(((IStatusCodeHttpResult)response).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task CreateConflict_Returns409()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.CreateConflict());

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Value, Is.EqualTo("A conflicting resource already exists."));
        Assert.That(((IStatusCodeHttpResult)response).StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
    }

    [Test]
    public async Task GetUnauthorized_Returns401()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.GetUnauthorized());

        Assert.That(response, Is.Not.Null);
        Assert.That(response, Is.TypeOf<UnauthorizedHttpResult>());
        Assert.That(((IStatusCodeHttpResult)response!).StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
    }

    [Test]
    public async Task Validate_ReturnsUnprocessableEntity()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.Validate());

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Value, Is.EqualTo("Data failed validation."));
        Assert.That(((IStatusCodeHttpResult)response).StatusCode, Is.EqualTo(StatusCodes.Status422UnprocessableEntity));
    }

    [Test]
    public async Task Teapot_Returns418()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.Teapot());

        Assert.That(response, Is.Not.Null);
        Assert.That(response, Is.TypeOf<StatusCodeHttpResult>());
        Assert.That(response!.StatusCode, Is.EqualTo(StatusCodes.Status418ImATeapot));
    }

    [Test]
    public async Task GetProblem_ReturnsProblemDetails()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.GetProblem());

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.ProblemDetails.Detail, Is.EqualTo("An unexpected error occurred."));
        Assert.That(response.ProblemDetails.Status, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task GetJson_ReturnsJsonResult()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.GetJson());

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Value!.Id, Is.EqualTo(99));
    }

    [Test]
    public async Task GetJson_ReturnsJsonResult_WithCall()
    {
        var result = await Client
            .Rait<TypedController>()
            .CallAsync(c => c.GetJson());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value!.Id, Is.EqualTo(99));
    }

    [Test]
    public async Task GetJson_ReturnsJsonResult_WithCallR()
    {
        var result = await Client
            .Rait<TypedController>()
            .CallRequiredAsync(c => c.GetJson());

        Assert.That(result.Value!.Id, Is.EqualTo(99));
    }
}

public sealed class RaitLegacyTypedResultsTests : RaitLegacyConfiguredTestBase
{
    [Test]
    public void GetAsyncResultsWithoutBody_ThrowsInLegacyMode()
    {
        Assert.ThrowsAsync<RaitHttpException>(async () =>
            await Client
                .Rait<TypedController>()
                .CallAsync(c => c.GetAsyncResultsWithoutBody(0)));
    }
}
