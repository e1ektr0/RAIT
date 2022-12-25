using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace RAIT.Core;

public class RaitHttpWrapper<TController> where TController : ControllerBase
{
    private readonly HttpClient _client;

    public RaitHttpWrapper(HttpClient client)
    {
        _client = client;
    }

    public async Task<TOutput?> Call<TOutput>(
        Expression<Func<TController, Task<TOutput>>> tree) where TOutput : class

    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree);
        var rout = RaitRouter.PrepareRout(tree, prepareInputParameters);
        return await RaitHttpRequester.HttpRequest<TOutput>(_client, methodInfo.CustomAttributes, rout,
            prepareInputParameters);
    }
}