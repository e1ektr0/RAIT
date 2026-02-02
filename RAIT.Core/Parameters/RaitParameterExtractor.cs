using System.Linq.Expressions;
using System.Reflection;

namespace RAIT.Core;

/// <summary>
/// Extracts parameters from expression trees representing controller method calls.
/// </summary>
internal static class RaitParameterExtractor
{
    internal static List<InputParameter> ExtractParameters<TInput, TOutput>(
        Expression<Func<TInput, Task<TOutput>>> expressionTree,
        MethodInfo method)
    {
        var methodCallExpression = (MethodCallExpression)expressionTree.Body;
        return ExtractMethodParameters(methodCallExpression, method);
    }

    internal static List<InputParameter> ExtractParameters<TInput, TOutput>(
        Expression<Func<TInput, TOutput>> expressionTree,
        MethodInfo method)
    {
        var methodCallExpression = (MethodCallExpression)expressionTree.Body;
        return ExtractMethodParameters(methodCallExpression, method);
    }

    internal static List<InputParameter> ExtractParameters<TInput>(
        Expression<Func<TInput>> expressionTree,
        MethodInfo method)
    {
        var methodCallExpression = (MethodCallExpression)expressionTree.Body;
        return ExtractMethodParameters(methodCallExpression, method);
    }

    internal static List<InputParameter> ExtractParameters<TInput>(
        Expression<Func<TInput, Task>> expressionTree,
        MethodInfo method)
    {
        var methodCallExpression = (MethodCallExpression)expressionTree.Body;
        return ExtractMethodParameters(methodCallExpression, method);
    }

    private static List<InputParameter> ExtractMethodParameters(MethodCallExpression methodCallExpression,
        MethodInfo method)
    {
        var parameters = new List<InputParameter>();
        var methodParameters = method.GetParameters();
        var argumentExpressions = methodCallExpression.Arguments;

        for (var i = 0; i < argumentExpressions.Count; i++)
        {
            var argumentExpression = argumentExpressions[i];
            var methodParameter = methodParameters[i];
            var value = ExpressionEvaluator.Evaluate(argumentExpression);

            if (value != null || argumentExpression is not ConstantExpression)
            {
                parameters.AddRange(InputParameterFactory.CreateFromValue(methodParameter, value));
            }
        }

        return parameters;
    }
}
