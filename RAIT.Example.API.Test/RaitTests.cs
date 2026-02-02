using System.Globalization;
using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;
using RAIT.Example.API.Test.Infrastructure;

namespace RAIT.Example.API.Test;

public sealed class RaitTests : RaitConfiguredTestBase
{
    [Test]
    public async Task GetWithId_ValidId_ReturnsExpectedResult()
    {
        await Client.Rait<RaitTestController>().Call(n => n.GetWithId(10));
    }

    [Test]
    public async Task Post_ValidModel_ReturnsExpectedResult()
    {
        var model = new Model { Id = 10 };
        var responseModel = await Client.Rait<RaitTestController>().Call(n => n.Post(model));

        Assert.That(responseModel!.Id, Is.EqualTo(10));
    }

    [Test]
    public async Task PostWithoutResponse_ValidModel()
    {
        var model = new Model { Id = 10 };
        await Client.Rait<RaitTestController>().CallR(n => n.PostWithoutResponse(model));
    }

    [Test]
    public async Task PutFromQuery_ValidModel_PerformsPutOperation()
    {
        var model = new Model { Id = 10 };
        await Client.Rait<RaitTestController>().Call(n => n.PutFromQuery(model));
    }

    [Test]
    public async Task GetFromQuery_ValidGuidModel_PerformsGetOperation()
    {
        var model = new Model { Guid = Guid.NewGuid() };
        await Client.Rait<RaitTestController>().Call(n => n.GetFromQuery(model));
    }

    [Test]
    public void SyncPut_ValidModel_PerformsPutOperation()
    {
        var model = new Model { Id = 10 };
        Client.Rait<RaitTestController>().Call(n => n.SyncPut(model));
    }

    [Test]
    public async Task DeleteQuery_ValidId_PerformsDeleteOperation()
    {
        await Client.Rait<RaitTestController>().Call(n => n.DeleteQuery(10));
    }

    [Test]
    public async Task DeleteQueryNamed_ValidId_PerformsDeleteOperation()
    {
        await Client.Rait<RaitTestController>().Call(n => n.DeleteQueryNamed(10));
    }

    [Test]
    public async Task Get_ActionRouting_ReturnsExpectedResult()
    {
        await Client.Rait<RaitActionRoutingTestController>().Call(n => n.Get(1));
    }

    [Test]
    public async Task GetReportsResult_ValidFilters_ReturnsSuccess()
    {
        var call = await Client.Rait<RaitFilterTestController>().CallR(n =>
            n.GetReportsResult("fff", "qqq", "sss"));
        Assert.That(call.Success, Is.True);
    }

    [Test]
    public async Task NullableEndpoints_CallsEndpoints_SuccessfulExecution()
    {
        await Client.Rait<RaitNullableTestController>().Call(n => n.Get());
        await Client.Rait<RaitNullableTestController>().Call(n => n.Post());
    }

    [Test]
    public async Task ActionResultEndpoints_CallsEndpoints_ReturnsExpectedResults()
    {
        await Client.Rait<RaitActionResultTestController>().Call(n => n.Get());

        Assert.ThrowsAsync<RaitHttpException>(async () =>
            await Client.Rait<RaitActionResultTestController>().Call(n => n.Get401StatusCode()));

        await Client.Rait<RaitActionResultTestController>().Call(n => n.Post(1));
    }

    [Test]
    public async Task Get_EnumResponse_ReturnsExpectedResult()
    {
        var response = await Client.Rait<RaitEnumTestController>().Call(n => n.Get());
        Assert.That(response, Is.Not.Null);
    }

    [Test]
    public async Task FormModelNull_ValidModel_ReturnsExpectedResult()
    {
        var model = new ModelWithNullValues { Id = 10 };
        var responseModel = await Client.Rait<RaitTestController>().Call(n => n.FormModelNull(model));
        Assert.That(responseModel!.Id, Is.EqualTo(model.Id));
    }

    [Test]
    public async Task GetChildResp_DerivedResponse_ReturnsExpectedResult()
    {
        var response = await Client.Rait<RaitDerivedResponseController>()
            .Call<ChildResp, BaseResp>(n => n.GetChildResp());
        Assert.That(response, Is.Not.Null);
    }

    [Test]
    public async Task Get_ResponseWithoutDeserialization_SuccessfulExecution()
    {
        var response = await Client.Rait<RaitTestController>().CallH(n => n.Get());
        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task GetWithArray_ValidGuidArray_ReturnsExpectedResult()
    {
        var request = new ArrayValueRequest
        {
            Array = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };
        var response = await Client.Rait<RaitTestController>().CallR(n => n.GetWithArray(request));
        Assert.That(response.Array, Is.Not.Empty);
    }

    [Test]
    public async Task GetWithDate_ValidDate_ReturnsExpectedResult()
    {
        var request = new DateTimeRequest { DateTime = DateTime.UtcNow };
        var response = await Client.Rait<RaitTestController>().CallR(n => n.GetWithDate(request));
        Assert.That(response.DateTime, Is.EqualTo(request.DateTime));
    }

    [Test]
    public async Task DateTimeOffsetInQuery_NoError()
    {
        await Client.Rait<DateTimeTypesController>().CallR(n => n.GetDateTimeOffset(DateTimeOffset.UtcNow));
    }

    [Test]
    public async Task DateTimeInQuery_NoError()
    {
        await Client.Rait<DateTimeTypesController>().CallR(n => n.GetDateTime(DateTime.UtcNow));
    }

    [Test]
    public async Task RouteBodyTest()
    {
        var request = new Model();
        await Client.Rait<RaitTestController>().CallR(n => n.RouteBody(1, request));
    }

    [Test]
    public async Task RouteQueryTest()
    {
        var request = new Model
        {
            Id = 5,
            Guid = Guid.NewGuid(),
            Domain = "qwerty",
            Bool = true,
            Decimal = 15m
        };
        await Client.Rait<RaitTestController>().CallR(n => n.RouteQuery(1, request));
    }

    [Test]
    public async Task GetWithGuid()
    {
        var request = Guid.NewGuid();
        var response = await Client.Rait<RaitTestController>().CallR(n => n.GetWithGuid(request));
        Assert.That(response, Is.EqualTo(request));
    }

    [Test]
    public async Task GetWithGuid_NullValue()
    {
        var request = (Guid?)null;
        var response = await Client.Rait<RaitTestController>()
            .Call(n => n.GetWithGuid(request));
        Assert.That(response, Is.EqualTo(request));
    }

    [Test]
    public async Task DateOnly_query_parameters_are_serialized_invariantly()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUi = CultureInfo.CurrentUICulture;

        // Simulate client running under a dd/MM/yyyy culture
        CultureInfo.CurrentCulture = new CultureInfo("en-GB");
        CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

        try
        {
            var expectedFrom = new DateOnly(2026, 1, 5); // 5 Jan 2026
            var expectedTo = new DateOnly(2026, 1, 6); // 6 Jan 2026

            var result = await Client.Rait<DateTimeTypesController>()
                .CallR(c => c.GetDateOnly(expectedFrom, expectedTo));

            Assert.That(result.Value!.From, Is.EqualTo(expectedFrom));
            Assert.That(result.Value.To, Is.EqualTo(expectedTo));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUi;
        }
    }
}
