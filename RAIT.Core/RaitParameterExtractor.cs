using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace RAIT.Core;

internal static class RaitParameterExtractor
{
    internal static List<InputParameter> PrepareInputParameters<TInput, TOutput>(
        Expression<Func<TInput, Task<TOutput>>> tree)
    {
        var methodBody = tree.Body as MethodCallExpression;
        return InputParameters(methodBody);
    }

    internal static List<InputParameter> PrepareInputParameters<TInput>(
        Expression<Func<TInput, Task>> tree)
    {
        var methodBody = tree.Body as MethodCallExpression;
        return InputParameters(methodBody);
    }

    private static List<InputParameter> InputParameters(MethodCallExpression? methodBody)
    {
        var methodInfo = methodBody!.Method;
        var parameterInfos = methodInfo.GetParameters();
        var arguments = methodBody.Arguments;

        var parameters = new List<InputParameter>();
        for (var index = 0; index < arguments.Count; index++)
        {
            var arg = arguments[index];
            var parameterInfo = parameterInfos[index];
            switch (arg)
            {
                case MemberInitExpression:
                    var func = Expression.Lambda<Func<object>>(arg).Compile();
                    var o = func();
                    parameters.Add(CreateParameter(parameterInfo, o));
                    break;
                case MemberExpression methodBodyArgument:
                {
                    var value = GetValue(methodBodyArgument);
                    if (value == null)
                        continue;
                    parameters.Add(CreateParameter(parameterInfo, value));
                    break;
                }
                case ConstantExpression constantExpression:
                {
                    var value = constantExpression.Value;
                    if (value == null)
                        continue;
                    parameters.Add(CreateParameter(parameterInfo, value));
                    break;
                }
            }
        }

        return parameters;
    }

    private static InputParameter CreateParameter(ParameterInfo info, object? value)
    {
        var type = value?.GetType()!;
        return new InputParameter
        {
            Value = value,
            Name = info.Name!,
            IsQuery = info.CustomAttributes.Any(n => n.AttributeType == typeof(FromQueryAttribute)) ||
                      type.IsValueType || type == typeof(string),
            IsForm = info.CustomAttributes.Any(n => n.AttributeType == typeof(FromFormAttribute)),
            Type = type
        };
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