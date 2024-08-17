using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace RAIT.Core;

public class RaitHttpClientWrapper<TController> where TController : ControllerBase
{
    private readonly HttpClient _client;

    public RaitHttpClientWrapper(HttpClient client)
    {
        _client = client;
    }

    public async Task<TOutput?> Call<TOutput, TOut>(Expression<Func<TController, Task<TOut>>> expression)
        where TOutput : TOut
    {
        var requestDetails =RequestPreparer<TController>.PrepareRequest(expression);
        var result = await RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client,
            requestDetails.CustomAttributes, requestDetails.Route, requestDetails.InputParameters);
        return result;
    }

    public async Task<TOutput?> Call<TOutput>(Expression<Func<TController, Task<TOutput>>> expression)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        var result = await RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client,
            requestDetails.CustomAttributes, requestDetails.Route, requestDetails.InputParameters);
        return result;
    }

    public async Task<TOutput> CallR<TOutput, TOut>(Expression<Func<TController, Task<TOut>>> expression)
        where TOutput : TOut
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        var result = await RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client,
            requestDetails.CustomAttributes, requestDetails.Route, requestDetails.InputParameters);
        return result ?? throw new ArgumentNullException();
    }

    public async Task<TOutput> CallR<TOutput>(Expression<Func<TController, Task<TOutput>>> expression)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        var result = await RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client,
            requestDetails.CustomAttributes, requestDetails.Route, requestDetails.InputParameters);
        return result ?? throw new ArgumentNullException();
    }

    public async Task Call(Expression<Func<TController, Task>> expression)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        await RaitHttpRequester<TController>.HttpRequest<EmptyResponse>(_client, requestDetails.CustomAttributes,
            requestDetails.Route, requestDetails.InputParameters);
    }

    public async Task CallR(Expression<Func<TController, Task>> expression)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        await RaitHttpRequester<TController>.HttpRequest<EmptyResponse>(_client, requestDetails.CustomAttributes,
            requestDetails.Route, requestDetails.InputParameters);
    }

    public async Task<HttpResponseMessage> CallH<TOut>(Expression<Func<TController, Task<TOut>>> expression)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        return await RaitHttpRequester<TController>.HttpRequest(_client, requestDetails.CustomAttributes,
            requestDetails.Route, requestDetails.InputParameters);
    }
}