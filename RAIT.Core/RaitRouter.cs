using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace RAIT.Core;

internal static class RaitRouter
{
    internal static string PrepareRout<TController, TOutput>(Expression<Func<TController, Task<TOutput>>> tree,
        List<InputParameter> generatedInputParameters) where TOutput : class
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;
        var result = "";
        var controllerType = typeof(TController);
        result += ConvertRout(controllerType.CustomAttributes, controllerType,
            generatedInputParameters) ?? "";
        result += "/";
        result += ConvertRout(methodInfo.CustomAttributes, controllerType,
            generatedInputParameters) ?? "";

        if (!generatedInputParameters.Any(n => n.IsQuery && !n.Used && n.Value != null))
            return result;
        result += "?";
        foreach (var generatedInputParameter in generatedInputParameters.Where(n =>
                     n.IsQuery && !n.Used && n.Value != null))
        {
            generatedInputParameter.Used = true;
            if (generatedInputParameter.Value == null)
                continue;
            result += PrepareValueToQuery(generatedInputParameter, generatedInputParameter.Name);
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

        // Get names for all IEnumerable properties (excl. string)
        var propertyNames = properties
            .Where(x => !(x.Value is string) && x.Value is IEnumerable)
            .Select(x => x.Key)
            .ToList();

        // Concat all IEnumerable properties into a comma separated string
        foreach (var key in propertyNames)
        {
            var valueType = properties[key]!.GetType();
            var valueElemType = valueType.IsGenericType
                ? valueType.GetGenericArguments()[0]
                : valueType.GetElementType();
            if (!valueElemType!.IsPrimitive && valueElemType != typeof(string))
                continue;

            var enumerable = properties[key] as IEnumerable;
            properties[key] = string.Join(separator, enumerable!.Cast<object>());
        }

        // Concat all key/value pairs into a string separated by ampersand
        return string.Join("&", properties
            .Select(x => string.Concat(
                Uri.EscapeDataString(x.Key), "=",
                Uri.EscapeDataString(x.Value!.ToString()!))));
    }
}