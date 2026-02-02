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

    public Task<TOutput?> CallAsync<TOutput, TOut>(Expression<Func<TController, Task<TOut>>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        where TOutput : TOut
    {
        return Call<TOutput, TOut>(expression, option);
    }

    public Task<TOutput?> CallAsync<TOutput>(Expression<Func<TController, Task<TOutput>>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        return Call(expression, option);
    }

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

    public Task<TOutput> CallRequiredAsync<TOutput, TOut>(Expression<Func<TController, Task<TOut>>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        where TOutput : TOut
    {
        return CallR<TOutput, TOut>(expression, option);
    }

    public Task<TOutput> CallRequiredAsync<TOutput>(Expression<Func<TController, Task<TOutput>>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        return CallR(expression, option);
    }

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

    public TOutput CallRequired<TOutput, TOut>(Expression<Func<TController, TOut>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        where TOutput : TOut
    {
        return CallR<TOutput, TOut>(expression, option);
    }

    public TOutput CallRequired<TOutput>(Expression<Func<TController, TOutput>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        return CallR(expression, option);
    }

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

    public Task CallAsync(Expression<Func<TController, Task>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        return Call(expression, option);
    }

    public Task CallAsync(Expression<Func<TController>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        return Call(expression, option);
    }

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

    public Task<HttpResponseMessage> CallHttpAsync<TOut>(Expression<Func<TController, Task<TOut>>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        return CallH(expression, option);
    }

    public HttpResponseMessage CallHttp<TOut>(Expression<Func<TController, TOut>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        return CallH(expression, option);
    }

    public Task<HttpResponseMessage> CallHttpAsync(Expression<Func<TController, Task>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        return CallH(expression, option);
    }

    public HttpResponseMessage CallHttp(Expression<Func<TController>> expression,
        HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
    {
        return CallH(expression, option);
    }

}
