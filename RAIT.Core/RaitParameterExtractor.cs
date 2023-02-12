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
                    throw new NotImplementedException("new Model() not support yet");
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
        return new InputParameter
        {
            Value = value,
            Name = info.Name!,
            IsQuery = info.CustomAttributes.Any(n => n.AttributeType == typeof(FromQueryAttribute)),
            IsForm = info.CustomAttributes.Any(n=>n.AttributeType == typeof(FromFormAttribute)),
            Type = value?.GetType()
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