using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace RAIT.Core;

internal static class RequestPreparer<TController> where TController : ControllerBase
{
    public static RequestDetails PrepareRequest<TOutput>(Expression<Func<TController, Task<TOutput>>> expression)
    {
        var methodCallExpr = expression.Body as MethodCallExpression;
        var methodInfo = methodCallExpr!.Method;
        var method = typeof(TController).GetMethod(methodCallExpr.Method.Name)!;

        var inputParameters = RaitParameterExtractor.PrepareInputParameters(expression, method);
        RaitDocumentationGenerator.Params<TController>(inputParameters);
        RaitDocumentationGenerator.Method<TController>(methodInfo.Name, inputParameters);

        var route = RaitRouter.PrepareRoute(expression, inputParameters);
        return new RequestDetails(inputParameters, route, method.CustomAttributes);
    }

    public static RequestDetails PrepareRequest(Expression<Func<TController, Task>> expression)
    {
        var methodCallExpr = expression.Body as MethodCallExpression;
        var methodInfo = methodCallExpr!.Method;
        var method = typeof(TController).GetMethod(methodCallExpr.Method.Name)!;

        var inputParameters = RaitParameterExtractor.PrepareInputParameters(expression, method);
        RaitDocumentationGenerator.Params<TController>(inputParameters);
        RaitDocumentationGenerator.Method<TController>(methodInfo.Name, inputParameters);

        var route = RaitRouter.PrepareRoute(expression, inputParameters);
        return new RequestDetails(inputParameters, route, method.CustomAttributes);
    }
}