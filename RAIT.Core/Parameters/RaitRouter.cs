using System.Linq.Expressions;
using System.Reflection;

namespace RAIT.Core;

/// <summary>
/// Builds routes from controller/method attributes and input parameters.
/// </summary>
internal static class RaitRouter
{
    internal static string PrepareRoute<TController, TOutput>(
        Expression<Func<TController, Task<TOutput>>> expression, List<InputParameter> inputParameters)
    {
        var methodCall = expression.Body as MethodCallExpression;
        return GenerateRoute<TController>(inputParameters, methodCall);
    }

    internal static string PrepareRoute<TController, TOutput>(
        Expression<Func<TController, TOutput>> expression, List<InputParameter> inputParameters)
    {
        var methodCall = expression.Body as MethodCallExpression;
        return GenerateRoute<TController>(inputParameters, methodCall);
    }

    internal static string PrepareRoute<TController>(
        Expression<Func<TController>> expression, List<InputParameter> inputParameters)
    {
        var methodCall = expression.Body as MethodCallExpression;
        return GenerateRoute<TController>(inputParameters, methodCall);
    }

    internal static string PrepareRoute<TController>(Expression<Func<TController, Task>> expression,
        List<InputParameter> inputParameters)
    {
        var methodCall = expression.Body as MethodCallExpression;
        return GenerateRoute<TController>(inputParameters, methodCall);
    }

    private static string GenerateRoute<TController>(List<InputParameter> inputParameters,
        MethodCallExpression? methodCall)
    {
        if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));

        var methodInfo = methodCall.Method;
        var controllerType = typeof(TController);
        methodInfo = controllerType.GetMethod(methodInfo.Name);
        var routeAttributes = controllerType.GetCustomAttributesData();
        var methodAttributes = methodInfo!.GetCustomAttributesData();

        var route = BuildControllerRoute(controllerType, routeAttributes, inputParameters);
        route = AppendMethodRoute(route, methodInfo, methodAttributes, inputParameters);
        route = route.Replace("[action]", methodInfo.Name);
        route += QueryStringBuilder.BuildQueryString(inputParameters);

        return route;
    }

    private static string BuildControllerRoute(Type controllerType, IList<CustomAttributeData> attributes,
        List<InputParameter> inputParameters)
    {
        return RouteTemplateProcessor.ProcessRoute(controllerType, attributes, inputParameters);
    }

    private static string AppendMethodRoute(string baseRoute, MemberInfo methodInfo,
        IList<CustomAttributeData> attributes, List<InputParameter> inputParameters)
    {
        var methodRoute = RouteTemplateProcessor.ProcessRoute(methodInfo, attributes, inputParameters);

        if (methodRoute.StartsWith("/"))
            return methodRoute;

        return baseRoute + "/" + methodRoute;
    }
}
