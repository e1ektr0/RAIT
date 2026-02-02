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

    #region Call - Async with Task<T> return, nullable output

    public async Task<TOutput?> Call<TOutput, TOut>(Expression<Func<TController, Task<TOut>>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        where TOutput : TOut
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        return await RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client,
            requestDetails.CustomAttributes, requestDetails.Route, requestDetails.InputParameters, option);
    }

    public async Task<TOutput?> Call<TOutput>(Expression<Func<TController, Task<TOutput>>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        return await RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client,
            requestDetails.CustomAttributes, requestDetails.Route, requestDetails.InputParameters, option);
    }

    #endregion

    #region Call - Sync return, nullable output

    public TOutput? Call<TOutput, TOut>(Expression<Func<TController, TOut>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        where TOutput : TOut
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        return RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client,
            requestDetails.CustomAttributes, requestDetails.Route, requestDetails.InputParameters, option).Result;
    }

    public TOutput? Call<TOutput>(Expression<Func<TController, TOutput>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        return RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client,
            requestDetails.CustomAttributes, requestDetails.Route, requestDetails.InputParameters, option).Result;
    }

    #endregion

    #region CallR - Async with Task<T> return, non-null output

    public async Task<TOutput> CallR<TOutput, TOut>(Expression<Func<TController, Task<TOut>>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        where TOutput : TOut
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        var result = await RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client,
            requestDetails.CustomAttributes, requestDetails.Route, requestDetails.InputParameters, option);
        return result ?? throw new ArgumentNullException();
    }

    public async Task<TOutput> CallR<TOutput>(Expression<Func<TController, Task<TOutput>>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        var result = await RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client,
            requestDetails.CustomAttributes, requestDetails.Route, requestDetails.InputParameters, option);
        return result ?? throw new ArgumentNullException();
    }

    #endregion

    #region CallR - Sync return, non-null output

    public TOutput CallR<TOutput, TOut>(Expression<Func<TController, TOut>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        where TOutput : TOut
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        var result = RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client,
            requestDetails.CustomAttributes, requestDetails.Route, requestDetails.InputParameters, option).Result;
        return result ?? throw new ArgumentNullException();
    }

    public TOutput CallR<TOutput>(Expression<Func<TController, TOutput>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        var result = RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client,
            requestDetails.CustomAttributes, requestDetails.Route, requestDetails.InputParameters, option).Result;
        return result ?? throw new ArgumentNullException();
    }

    #endregion

    #region Call - Void returns

    public async Task Call(Expression<Func<TController, Task>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        await RaitHttpRequester<TController>.HttpRequest<EmptyResponse>(_client, requestDetails.CustomAttributes,
            requestDetails.Route, requestDetails.InputParameters, option);
    }

    public async Task Call(Expression<Func<TController>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        await RaitHttpRequester<TController>.HttpRequest<EmptyResponse>(_client, requestDetails.CustomAttributes,
            requestDetails.Route, requestDetails.InputParameters, option);
    }

    #endregion

    #region CallH - Returns raw HttpResponseMessage

    public async Task<HttpResponseMessage> CallH<TOut>(Expression<Func<TController, Task<TOut>>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        return await RaitHttpRequester<TController>.HttpRequest(_client, requestDetails.CustomAttributes,
            requestDetails.Route, requestDetails.InputParameters, option);
    }

    public HttpResponseMessage CallH<TOut>(Expression<Func<TController, TOut>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        return RaitHttpRequester<TController>.HttpRequest(_client, requestDetails.CustomAttributes,
            requestDetails.Route, requestDetails.InputParameters, option).Result;
    }

    public async Task<HttpResponseMessage> CallH(Expression<Func<TController, Task>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        return await RaitHttpRequester<TController>.HttpRequest(_client, requestDetails.CustomAttributes,
            requestDetails.Route, requestDetails.InputParameters, option);
    }

    public HttpResponseMessage CallH(Expression<Func<TController>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        var requestDetails = RequestPreparer<TController>.PrepareRequest(expression);
        return RaitHttpRequester<TController>.HttpRequest(_client, requestDetails.CustomAttributes,
            requestDetails.Route, requestDetails.InputParameters, option).Result;
    }

    #endregion
}
