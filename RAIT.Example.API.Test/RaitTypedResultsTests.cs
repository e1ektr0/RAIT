using System.Net;
using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Test.Infrastructure;

namespace RAIT.Example.API.Test;

public sealed class RaitTypedResultsTests : RaitConfiguredTestBase
{
    [Test]
    public async Task GetAsyncResults_ReturnsOk()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallH(c => c.GetAsyncResults(1));

        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task GetCreated_ReturnsCreated()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallH(c => c.GetCreated(1));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task GetAccepted_ReturnsAccepted()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallR(c => c.GetAccepted());

        Assert.That(response.Value!.Id, Is.EqualTo(42));
    }

    [Test]
    public async Task Delete_ReturnsNoContent_WhenExists()
    {
        await Client
            .Rait<TypedController>()
            .Call(c => c.Delete(5));
    }

    [Test]
    public async Task CreateBadRequest_Returns400()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallH(c => c.CreateBadRequest());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateConflict_Returns409()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallH(c => c.CreateConflict());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task GetUnauthorized_Returns401()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallH(c => c.GetUnauthorized());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Validate_ReturnsUnprocessableEntity()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallH(c => c.Validate());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }

    [Test]
    public async Task Teapot_Returns418()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallH(c => c.Teapot());

        Assert.That((int)response.StatusCode, Is.EqualTo(418));
    }

    [Test]
    public async Task GetProblem_ReturnsProblemDetails()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallH(c => c.GetProblem());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task GetJson_ReturnsJsonResult()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallH(c => c.GetJson());

        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task GetJson_ReturnsJsonResult_WithCall()
    {
        var result = await Client
            .Rait<TypedController>()
            .Call(c => c.GetJson());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value!.Id, Is.EqualTo(99));
    }

    [Test]
    public async Task GetJson_ReturnsJsonResult_WithCallR()
    {
        var result = await Client
            .Rait<TypedController>()
            .CallR(c => c.GetJson());

        Assert.That(result.Value!.Id, Is.EqualTo(99));
    }
}
