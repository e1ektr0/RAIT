using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace RAIT.Core;

public class EmptyResponse
{
}

public class RaitHttpWrapper<TController> where TController : ControllerBase
{
    private readonly HttpClient _client;

    public RaitHttpWrapper(HttpClient client)
    {
        _client = client;
    }

    private (List<InputParameter> prepareInputParameters, string rout, IEnumerable<CustomAttributeData>
        CustomAttributes)
        PrepareRequest<TOutput>(Expression<Func<TController, Task<TOutput>>> tree)
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;
        var method = typeof(TController).GetMethod(methodBody.Method.Name);

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree, method);
        RaitDocumentationGenerator.Params<TController>(prepareInputParameters);
        RaitDocumentationGenerator.Method<TController>(methodInfo.Name, prepareInputParameters);

        var rout = RaitRouter.PrepareRoute(tree, prepareInputParameters);
        return (prepareInputParameters, rout, method!.CustomAttributes);
    }

    private (List<InputParameter> prepareInputParameters, string rout,
        IEnumerable<CustomAttributeData> CustomAttributes) PrepareRequest(Expression<Func<TController, Task>> tree)
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;
        var method = typeof(TController).GetMethod(methodBody.Method.Name);

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree, method);
        RaitDocumentationGenerator.Params<TController>(prepareInputParameters);
        RaitDocumentationGenerator.Method<TController>(methodInfo.Name, prepareInputParameters);

        var rout = RaitRouter.PrepareRoute(tree, prepareInputParameters);
        return (prepareInputParameters, rout, method!.CustomAttributes);
    }


    public async Task<TOutput?> Call<TOutput, TOut>(Expression<Func<TController, Task<TOut>>> tree) where TOutput : TOut
    {
        var (prepareInputParameters, rout, attributes) = PrepareRequest(tree);
        var result =
            await RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client, attributes, rout,
                prepareInputParameters);
        return result;
    }

    public async Task<TOutput?> Call<TOutput>(Expression<Func<TController, Task<TOutput>>> tree)
    {
        var (prepareInputParameters, rout, attributes) = PrepareRequest(tree);
        var result =
            await RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client, attributes, rout,
                prepareInputParameters);
        return result;
    }

    public async Task<TOutput> CallR<TOutput, TOut>(Expression<Func<TController, Task<TOut>>> tree) where TOutput : TOut
    {
        var (prepareInputParameters, rout, attributes) = PrepareRequest(tree);
        var result =
            await RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client, attributes, rout,
                prepareInputParameters);
        return result ?? throw new ArgumentNullException();
    }

    public async Task<TOutput> CallR<TOutput>(Expression<Func<TController, Task<TOutput>>> tree)
    {
        var (prepareInputParameters, rout, attributes) = PrepareRequest(tree);
        var result =
            await RaitHttpRequester<TController>.HttpRequest<TOutput?>(_client, attributes, rout,
                prepareInputParameters);
        return result ?? throw new ArgumentNullException();
    }

    public async Task Call(Expression<Func<TController, Task>> tree)
    {
        var (prepareInputParameters, rout, attributes) = PrepareRequest(tree);
        await RaitHttpRequester<TController>.HttpRequest<EmptyResponse>(_client, attributes, rout,
            prepareInputParameters);
    }

    public async Task CallR(Expression<Func<TController, Task>> tree)
    {
        var (prepareInputParameters, rout, attributes) = PrepareRequest(tree);
        await RaitHttpRequester<TController>.HttpRequest<EmptyResponse>(_client, attributes, rout,
            prepareInputParameters);
    }

    public async Task<HttpResponseMessage> CallH<TOut>(Expression<Func<TController, Task<TOut>>> tree)
    {
        var (prepareInputParameters, rout, attributes) = PrepareRequest(tree);
        return await RaitHttpRequester<TController>.HttpRequest(_client, attributes, rout, prepareInputParameters);
    }
}