using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace RAIT.Core;

internal static class RaitParameterExtractor
{
    internal static List<GeneratedInputParameter> PrepareInputParameters<TInput, TOutput>(
        Expression<Func<TInput, Task<TOutput>>> tree) where TOutput : class
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;
        var parameterInfos = methodInfo.GetParameters();
        var arguments = methodBody.Arguments;

        var parameters = new List<GeneratedInputParameter>();
        for (var index = 0; index < arguments.Count; index++)
        {
            var arg = arguments[index];
            var parameterInfo = parameterInfos[index];
            switch (arg)
            {
                case MemberExpression methodBodyArgument:
                {
                    var value = GetValue(methodBodyArgument);
                    if (value == null)
                        continue;
                    parameters.Add(new GeneratedInputParameter
                    {
                        Value = value,
                        Name = parameterInfo.Name!,
                        IsQuery = parameterInfo.CustomAttributes.Any(n => n.AttributeType == typeof(FromQueryAttribute))
                    });
                    break;
                }
                case ConstantExpression constantExpression:
                {
                    var value = constantExpression.Value;
                    if (value == null)
                        continue;

                    parameters.Add(new GeneratedInputParameter
                    {
                        Value = value, //as it is
                        Name = parameterInfo.Name!,
                        IsQuery = parameterInfo.CustomAttributes.Any(n => n.AttributeType == typeof(FromQueryAttribute))
                    });
                    break;
                }
            }
        }

        return parameters;
    }


    // ReSharper disable once ReturnTypeCanBeNotNullable
    private static object? GetValue(MemberExpression member)
    {
        var objectMember = Expression.Convert(member, typeof(object));

        var getterLambda = Expression.Lambda<Func<object>>(objectMember);

        var getter = getterLambda.Compile();

        return getter();
    }
}