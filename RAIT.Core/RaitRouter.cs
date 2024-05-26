using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace RAIT.Core;

internal static class RaitRouter
{
    internal static string PrepareRout<TController, TOutput>(Expression<Func<TController, Task<TOutput>>> tree,
        List<InputParameter> generatedInputParameters)
    {
        var methodBody = tree.Body as MethodCallExpression;
        return Result<TController>(generatedInputParameters, methodBody);
    }

    internal static string PrepareRout<TController>(Expression<Func<TController, Task>> tree,
        List<InputParameter> generatedInputParameters)
    {
        var methodBody = tree.Body as MethodCallExpression;
        return Result<TController>(generatedInputParameters, methodBody);
    }

    private static string Result<TController>(List<InputParameter> generatedInputParameters,
        MethodCallExpression? methodBody)
    {
        var methodInfo = methodBody!.Method;
        var result = "";
        var controllerType = typeof(TController);
        result += ConvertRout(controllerType.CustomAttributes, controllerType,
            generatedInputParameters) ?? "";
        result += "/";
        result += ConvertRout(methodInfo.CustomAttributes, controllerType,
            generatedInputParameters) ?? "";
        result = result.Replace("[action]", methodInfo.Name);

        if (!generatedInputParameters.Any(n => n.IsQuery && !n.Used && n.Value != null))
            return result;
        result += "?";
        bool first = true;
        foreach (var generatedInputParameter in generatedInputParameters.Where(n =>
                     n.IsQuery && !n.Used && n.Value != null))
        {
            generatedInputParameter.Used = true;
            if (generatedInputParameter.Value == null)
                continue;
            if (!first)
                result += "&";
            result += PrepareValueToQuery(generatedInputParameter, generatedInputParameter.Name);
            first = false;
        }

        return result;
    }

    private static string? ConvertRout(IEnumerable<CustomAttributeData> attributes,
        MemberInfo controllerType, List<InputParameter> generatedInputParameters)
    {
        var customAttributeData =
            attributes.FirstOrDefault(n => n.AttributeType == typeof(RouteAttribute));
        if (customAttributeData == null) return null;
        if (!customAttributeData.ConstructorArguments.Any()) return null;
        var customAttributeNamedArgument = customAttributeData.ConstructorArguments.FirstOrDefault();
        if (customAttributeNamedArgument.Value == null) return null;
        var convertRout = ((string)customAttributeNamedArgument.Value!).Replace("[controller]",
            controllerType.Name.Replace("Controller", ""));
        foreach (var generatedInputParameter in generatedInputParameters)
        {
            if (generatedInputParameter.Value == null)
                continue;

            var preparedRout = convertRout
                .Replace(":guid}", "}")
                .Replace(":int}", "}")
                .Replace(":long}", "}")
                .Replace(":string}", "}");
            var changed = preparedRout
                .Replace($"{{{generatedInputParameter.Name}}}",
                    generatedInputParameter.Value.ToString());
            if (changed != preparedRout)
                generatedInputParameter.Used = true;
            convertRout = changed;
        }

        return convertRout;
    }


    private static string PrepareValueToQuery(InputParameter inputParameter, string name)
    {
        if (inputParameter.Value is string value)
            return name + "=" + value;
        if (inputParameter.Value!.GetType().IsValueType)
            return name + "=" + inputParameter.Value!;

        return ToQueryString(inputParameter.Value);
    }

    private static string ToQueryString(this object request, string separator = ",")
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Get all properties on the object
        var properties = request.GetType().GetProperties()
            .Where(x => x.CanRead)
            .Where(x => x.GetValue(request, null) != null)
            .ToDictionary(x => x.Name, x => x.GetValue(request, null));

        var notArrayProperties = properties.Where(x => x.Value is not IEnumerable or string);
        var queryParams = notArrayProperties.Select(x =>
            string.Concat(Uri.EscapeDataString(x.Key), "=", ValueToString(x.Value!))).ToList();

        // Get names for all IEnumerable properties (excl. string, Guid)
        var propertyNames = properties
            .Where(x => x.Value is not string && x.Value is not Guid && x.Value is IEnumerable)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in propertyNames)
        {
            var valueType = properties[key]!.GetType();
            var valueElemType = valueType.IsGenericType
                ? valueType.GetGenericArguments()[0]
                : valueType.GetElementType();
            if (!valueElemType!.IsPrimitive && valueElemType != typeof(string) && valueElemType != typeof(Guid))
                continue;

            var enumerable = properties[key] as IEnumerable;
            queryParams.AddRange(enumerable!.Cast<object>().Select(n => $"{key}={n}"));
        }

        return string.Join("&", queryParams);
    }

    private static string ValueToString(object value)
    {
        if (value is DateTime dt)
            return dt.ToString("O");
        return Uri.EscapeDataString(value.ToString()!);
    }
}