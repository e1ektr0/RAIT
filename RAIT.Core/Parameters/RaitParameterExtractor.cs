using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace RAIT.Core;

internal static class RaitParameterExtractor
{
    internal static List<InputParameter> PrepareInputParameters<TInput, TOutput>(
        Expression<Func<TInput, Task<TOutput>>> tree,
        MethodInfo method)
    {
        var methodBody = tree.Body as MethodCallExpression;
        return InputParameters(methodBody, method);
    }

    internal static List<InputParameter> PrepareInputParameters<TInput>(Expression<Func<TInput, Task>> tree,
        MethodInfo method)
    {
        var methodBody = tree.Body as MethodCallExpression;
        return InputParameters(methodBody, method);
    }

    private static List<InputParameter> InputParameters(MethodCallExpression? methodBody, MethodInfo method)
    {
        var parameterInfos = method.GetParameters();
        var arguments = methodBody!.Arguments;

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
                    parameters.AddRange(CreateParameter(parameterInfo, o));
                    break;
                case MemberExpression methodBodyArgument:
                {
                    var value = GetValue(methodBodyArgument);
                    if (value == null)
                        continue;
                    parameters.AddRange(CreateParameter(parameterInfo, value));
                    break;
                }
                case ConstantExpression constantExpression:
                {
                    var value = constantExpression.Value;
                    if (value == null)
                        continue;
                    parameters.AddRange(CreateParameter(parameterInfo, value));
                    break;
                }
            }
        }

        return parameters;
    }

    private static bool IsParameter(IEnumerable<CustomAttributeData> attributes)
    {
        return attributes.Any(n =>
            n.AttributeType == typeof(FromQueryAttribute) ||
            n.AttributeType == typeof(FromFormAttribute) ||
            n.AttributeType == typeof(FromRouteAttribute) ||
            n.AttributeType == typeof(FromBodyAttribute));
    }

    private static bool IsValueParameter(Type type)
    {
        return type.IsValueType || type == typeof(string);
    }

    private static List<InputParameter> CreateParameter(ParameterInfo info, object? value)
    {
        var result = new List<InputParameter>();
        var type = value?.GetType()!;
        var isValueParameter = IsValueParameter(type);

        if (!IsParameter(info.CustomAttributes) && !isValueParameter)
        {
            if (value != null)
            {
                var fieldInfos = value.GetType().GetProperties();
                foreach (var fieldInfo in fieldInfos)
                {
                    if (IsParameter(fieldInfo.CustomAttributes))
                    {
                        var isQuery =
                            fieldInfo.CustomAttributes.Any(n => n.AttributeType == typeof(FromQueryAttribute));
                        var o = fieldInfo.GetValue(value);
                        if (o != null && isQuery && !IsValueParameter(o.GetType()) && !o.GetType().IsArray)
                        {
                            result.AddRange(o.GetType()
                                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(p => p.GetIndexParameters().Length == 0) // Ensure property is not an indexer
                                .Select(p => new InputParameter
                                {
                                    Value = p.GetValue(o)?.ToString(),
                                    Name = $"{fieldInfo.Name}.{p.Name}",
                                    IsQuery = true,
                                    Type = p.PropertyType
                                }));
                        }
                        else
                            result.Add(new InputParameter
                            {
                                Value = o,
                                Name = fieldInfo.Name,
                                IsQuery =
                                    isQuery,
                                IsForm = fieldInfo.CustomAttributes.Any(n =>
                                    n.AttributeType == typeof(FromFormAttribute)),
                                IsBody = fieldInfo.CustomAttributes.Any(n =>
                                    n.AttributeType == typeof(FromBodyAttribute)),
                                Type = fieldInfo.PropertyType
                            });
                    }
                }
            }
        }

        if (result.Any(n => n.IsBody))
            return result;

        result.Add(new InputParameter
        {
            Value = value,
            Name = info.Name!,
            IsQuery = info.CustomAttributes.Any(n => n.AttributeType == typeof(FromQueryAttribute)) ||
                      isValueParameter,
            IsForm = info.CustomAttributes.Any(n => n.AttributeType == typeof(FromFormAttribute)),
            Type = type
        });
        return result;
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