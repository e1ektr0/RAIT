using System.Collections;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace RAIT.Core;

public static class RaitExtensions
{
    public static async Task<TOutput?> Call<TController, TOutput>(this HttpClient client,
        Expression<Func<TController, Task<TOutput>>> tree) where TOutput : class where TController : ControllerBase

    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;

        var prepareInputParameters = PrepareInputParameters(tree);
        var rout = PrepareRout(tree, prepareInputParameters);
        return await HttpRequest<TOutput>(client, methodInfo.CustomAttributes, rout, prepareInputParameters);
    }

    private static async Task<TOutput?> HttpRequest<TOutput>(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string rout,
        List<GeneratedInputParameter> prepareInputParameters) where TOutput : class
    {
        var customAttributeData =
            attributes.FirstOrDefault(n => n.AttributeType.BaseType == typeof(HttpMethodAttribute));
        if (customAttributeData == null)
            throw new Exception("Http type attribute not found");

        if (customAttributeData.AttributeType == typeof(HttpGetAttribute))
        {
            var httpResponseMessage = await httpClient.GetAsync(rout);
            httpResponseMessage.EnsureSuccessStatusCode();
            return await httpResponseMessage.Content.ReadFromJsonAsync<TOutput>();
        }

        if (customAttributeData.AttributeType == typeof(HttpPostAttribute))
        {
            var generatedInputParameter = prepareInputParameters.FirstOrDefault(n => !n.Used);
            var jsonContent = JsonContent.Create(generatedInputParameter?.Value ?? "{}");
            var httpResponseMessage = await httpClient.PostAsync(rout, jsonContent);
            httpResponseMessage.EnsureSuccessStatusCode();
            return await httpResponseMessage.Content.ReadFromJsonAsync<TOutput>();
        }

        if (customAttributeData.AttributeType == typeof(HttpPutAttribute))
        {
            var generatedInputParameter = prepareInputParameters.FirstOrDefault(n => !n.Used);

            var jsonContent = JsonContent.Create(generatedInputParameter?.Value ?? "{}");
            if (generatedInputParameter == null)
                jsonContent = null;
            var httpResponseMessage = await httpClient.PutAsync(rout, jsonContent);
            httpResponseMessage.EnsureSuccessStatusCode();
            return await httpResponseMessage.Content.ReadFromJsonAsync<TOutput>();
        }

        if (customAttributeData.AttributeType == typeof(HttpDeleteAttribute))
        {
            var httpResponseMessage = await httpClient.DeleteAsync(rout);
            httpResponseMessage.EnsureSuccessStatusCode();
            return await httpResponseMessage.Content.ReadFromJsonAsync<TOutput>();
        }

        throw new Exception("wtf");
    }

    private static List<GeneratedInputParameter> PrepareInputParameters<TInput, TOutput>(
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


    private static object? GetValue(MemberExpression member)
    {
        var objectMember = Expression.Convert(member, typeof(object));

        var getterLambda = Expression.Lambda<Func<object>>(objectMember);

        var getter = getterLambda.Compile();

        return getter();
    }

    private static string PrepareRout<TController, TOutput>(Expression<Func<TController, Task<TOutput>>> tree,
        List<GeneratedInputParameter> generatedInputParameters) where TOutput : class
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

        if (generatedInputParameters.Any(n => n.IsQuery && !n.Used && n.Value != null))
        {
            result += "?";
            foreach (var generatedInputParameter in generatedInputParameters.Where(n =>
                         n.IsQuery && !n.Used && n.Value != null))
            {
                generatedInputParameter.Used = true;
                if (generatedInputParameter.Value == null)
                    continue;
                result += PrepareValueToQuery(generatedInputParameter, generatedInputParameter.Name);
            }
        }

        return result;
    }

    private static string PrepareValueToQuery(GeneratedInputParameter generatedInputParameter, string name)
    {
        if (generatedInputParameter.Value is string value)
            return name + "=" + value;
        if (generatedInputParameter.Value!.GetType().IsValueType)
            return  name + "=" + generatedInputParameter.Value!;

        return ToQueryString(generatedInputParameter.Value);
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
            if (valueElemType!.IsPrimitive || valueElemType == typeof(string))
            {
                var enumerable = properties[key] as IEnumerable;
                properties[key] = string.Join(separator, enumerable!.Cast<object>());
            }
        }

        // Concat all key/value pairs into a string separated by ampersand
        return string.Join("&", properties
            .Select(x => string.Concat(
                Uri.EscapeDataString(x.Key), "=",
                Uri.EscapeDataString(x.Value!.ToString()!))));
    }

    private static string? ConvertRout(IEnumerable<CustomAttributeData> attributes,
        Type controllerType, List<GeneratedInputParameter> generatedInputParameters)
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

            var changed = convertRout.Replace($"{{{generatedInputParameter.Name}}}",
                generatedInputParameter.Value.ToString());
            if (changed != convertRout)
                generatedInputParameter.Used = true;
            convertRout = changed;
        }

        return convertRout;
    }


    private class GeneratedInputParameter
    {
        public object? Value { get; init; }
        public string Name { get; init; } = null!;
        public bool Used { get; set; }
        public bool IsQuery { get; init; }
    }
}