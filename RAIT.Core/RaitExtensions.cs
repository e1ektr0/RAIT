using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace RAIT.Core;

public static class RaitExtensions
{
    public static async Task<TOutput?> Call<TController, TOutput>(this HttpClient client,
        Expression<Func<TController, Task<TOutput>>> tree) where TController : ControllerBase

    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree);
        var rout = RaitRouter.PrepareRout(tree, prepareInputParameters);
        return await RaitHttpRequester.HttpRequest<TOutput>(client, methodInfo.CustomAttributes, rout,
            prepareInputParameters);
    }
    
    public static  RaitHttpWrapper<TController> Rait<TController>(this HttpClient client) where TController : ControllerBase
    {
        return new RaitHttpWrapper<TController>(client);
    }
}

