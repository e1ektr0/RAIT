using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RAIT.Core;

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
            parameters.AddRange(EvaluateArgumentExpression(methodParameter, argumentExpression));
        }

        return parameters;
    }

    private static IEnumerable<InputParameter> EvaluateArgumentExpression(ParameterInfo parameterInfo,
        Expression arg)
    {
        IEnumerable<InputParameter> result = arg switch
        {
            MemberInitExpression => ExtractParametersFromMemberInitExpression(parameterInfo, arg),
            MemberExpression memberExpr => ExtractParametersFromMemberExpression(parameterInfo, memberExpr),
            ConstantExpression constantExpr => ExtractParametersFromConstantExpression(parameterInfo, constantExpr),
            UnaryExpression unaryExpr => ExtractParametersFromUnaryExpression(parameterInfo, unaryExpr),
            _ => Enumerable.Empty<InputParameter>()
        };
        return result;
    }

    private static IEnumerable<InputParameter> ExtractParametersFromMemberInitExpression(
        ParameterInfo parameterInfo, Expression arg)
    {
        var lambda = Expression.Lambda<Func<object>>(arg);
        var compiledLambda = lambda.Compile();
        var value = compiledLambda();

        return CreateInputParametersFromValue(parameterInfo, value);
    }

    private static IEnumerable<InputParameter> ExtractParametersFromMemberExpression(ParameterInfo parameterInfo,
        MemberExpression memberExpr)
    {
        var value = ExtractValueFromExpression(memberExpr);
        return CreateInputParametersFromValue(parameterInfo, value);
    }
    
    private static IEnumerable<InputParameter> ExtractParametersFromUnaryExpression(ParameterInfo parameterInfo,
        UnaryExpression unaryExpr)
    {
        var value = ExtractValueFromUnaryExpression(unaryExpr);
        return CreateInputParametersFromValue(parameterInfo, value);
    }

    private static IEnumerable<InputParameter> ExtractParametersFromConstantExpression(ParameterInfo parameterInfo,
        ConstantExpression constantExpr)
    {
        var value = constantExpr.Value;
        return value != null
            ? CreateInputParametersFromValue(parameterInfo, value)
            : Enumerable.Empty<InputParameter>();
    }

    private static object ExtractValueFromExpression(MemberExpression memberExpression)
    {
        var convertedExpression = Expression.Convert(memberExpression, typeof(object));
        var getterLambda = Expression.Lambda<Func<object>>(convertedExpression);
        var getter = getterLambda.Compile();
        return getter();
    }
    
    private static object ExtractValueFromUnaryExpression(UnaryExpression unaryExpr)
    {
        var convertedExpression = Expression.Convert(unaryExpr, typeof(object));
        var getterLambda = Expression.Lambda<Func<object>>(convertedExpression);
        var getter = getterLambda.Compile();
        return getter();
    }

    private static List<InputParameter> CreateInputParametersFromValue(ParameterInfo parameterInfo, object? value)
    {
        var inputParameters = new List<InputParameter>();

        if (value != null && IsComplexParameter(value) && !IsHttpParameter(parameterInfo.CustomAttributes) &&
            value.GetType() != typeof(RaitFormFile))
        {
            inputParameters.AddRange(
                from property in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where property.GetIndexParameters().Length == 0
                let propertyValue = property.GetValue(value)
                from parameter in CreateParameterSetForProperty(property, propertyValue)
                select parameter);
        }
        else
        {
            inputParameters.Add(CreateBasicInputParameter(parameterInfo, value));
        }

        return inputParameters;
    }

    private static bool IsHttpParameter(IEnumerable<CustomAttributeData> customAttributes)
    {
        return customAttributes.Any(attr =>
            typeof(FromQueryAttribute).IsAssignableFrom(attr.AttributeType) ||
            typeof(FromFormAttribute).IsAssignableFrom(attr.AttributeType) ||
            typeof(FromHeaderAttribute).IsAssignableFrom(attr.AttributeType) ||
            typeof(FromRouteAttribute).IsAssignableFrom(attr.AttributeType) ||
            typeof(FromBodyAttribute).IsAssignableFrom(attr.AttributeType));
    }

    private static bool IsSimpleType(Type type) => type.IsValueType || type == typeof(string);

    private static bool IsPotentialQueryParameter(IEnumerable<CustomAttributeData> customAttributes)
    {
        return customAttributes.Any(attr =>
            typeof(FromQueryAttribute).IsAssignableFrom(attr.AttributeType));
    }

    private static bool IsComplexParameter(object parameterValue)
    {
        var parameterType = parameterValue.GetType();
        return !IsSimpleType(parameterType) && !parameterType.IsArray;
    }

    private static IEnumerable<InputParameter> CreateParameterSetForProperty(PropertyInfo property, object? value)
    {
        if (value == null)
            return Enumerable.Empty<InputParameter>();

        var isQueryParameter = IsPotentialQueryParameter(property.CustomAttributes);
        if (isQueryParameter && !IsSimpleType(value.GetType()))
        {
            return value.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetIndexParameters().Length == 0) // Ensure property is not an indexer
                .Select(p =>
                {
                    var name = GetNameFromAttribute(p);

                    return new InputParameter
                    {
                        Value = p.GetValue(value)?.ToString(),
                        Name = $"{property.Name}.{name ?? p.Name}",
                        IsQuery = true,
                        Type = p.PropertyType
                    };
                });
        }

        var isForm = property.CustomAttributes
            .Any(attr => typeof(FromFormAttribute).IsAssignableFrom(attr.AttributeType));
        var isBody = property.CustomAttributes
            .Any(attr => typeof(FromBodyAttribute).IsAssignableFrom(attr.AttributeType));
        var isHeader = property.CustomAttributes
            .Any(attr => typeof(FromHeaderAttribute).IsAssignableFrom(attr.AttributeType));
        var name = GetNameFromAttribute(property);
        return new List<InputParameter>
        {
            new()
            {
                Value = value,
                Name = name ?? property.Name,
                IsQuery = isQueryParameter,
                IsForm = isForm,
                IsBody = isBody,
                IsHeader = isHeader,
                Type = property.PropertyType
            }
        };
    }

    private static InputParameter CreateBasicInputParameter(ParameterInfo parameterInfo, object? value)
    {
        var isSimple = IsSimpleType(parameterInfo.ParameterType);
        var isQuery =
            parameterInfo.CustomAttributes.Any(attr =>
                typeof(FromQueryAttribute).IsAssignableFrom(attr.AttributeType)) ||
            isSimple;
        var name = GetNameFromAttribute(parameterInfo);
        return new InputParameter
        {
            Value = value,
            Name = name ?? parameterInfo.Name!,
            IsQuery = isQuery,
            IsForm = parameterInfo.CustomAttributes.Any(attr =>
                typeof(FromFormAttribute).IsAssignableFrom(attr.AttributeType)),
            Type = value?.GetType() ?? parameterInfo.ParameterType,
            IsHeader = parameterInfo.CustomAttributes.Any(attr =>
                typeof(FromHeaderAttribute).IsAssignableFrom(attr.AttributeType)),
        };
    }
    private static string? ExtractName(CustomAttributeData nameAttribute)
    {
        if (nameAttribute.NamedArguments.Any(n => n.MemberName == "Name"))
        {
            var customAttributeNamedArgument = nameAttribute.NamedArguments.First(n => n.MemberName == "Name");
            return (string?)customAttributeNamedArgument.TypedValue.Value;
        }

        // Use reflection to instantiate the attribute and access its properties
        var args = nameAttribute.ConstructorArguments.Select(arg => arg.Value).ToArray();
        if (Activator.CreateInstance(nameAttribute.AttributeType, args) is IModelNameProvider attributeInstance)
        {
            return attributeInstance.Name;
        }

        return null;
    }

    private static CustomAttributeData? GetNameAttribute(IEnumerable<CustomAttributeData> customAttributes)
    {
        return customAttributes.FirstOrDefault(n => n.AttributeType.GetInterfaces().Contains(typeof(IModelNameProvider)));
    }

    private static string? GetNameFromAttribute(ParameterInfo parameterInfo)
    {
        var nameAttribute = GetNameAttribute(parameterInfo.GetCustomAttributesData());
        return nameAttribute != null ? ExtractName(nameAttribute) : null;
    }

    private static string? GetNameFromAttribute(PropertyInfo propertyInfo)
    {
        var nameAttribute = GetNameAttribute(propertyInfo.GetCustomAttributesData());
        return nameAttribute != null ? ExtractName(nameAttribute) : null;
    }
}