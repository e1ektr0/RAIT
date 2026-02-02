using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace RAIT.Core;

/// <summary>
/// Processes HTTP responses and deserializes them into the requested types.
/// </summary>
internal static class ResponseProcessor
{
    internal static async Task<object?> ProcessResponse(Type responseType, HttpResponseMessage httpResponseMessage)
    {
        if (httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
            return null;

        var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

        // Handle primitive and special types
        if (TryProcessPrimitiveType(responseType, responseContent, out var primitiveResult))
            return primitiveResult;

        return await DeserializeResponse(responseType, httpResponseMessage);
    }

    private static bool TryProcessPrimitiveType(Type responseType, string responseContent, out object? result)
    {
        result = null;

        if (responseType == typeof(EmptyResponse))
            return true;

        if (responseType == typeof(string) || responseType == typeof(object))
        {
            result = responseContent;
            return true;
        }

        if (responseType == typeof(Guid))
        {
            result = Guid.Parse(responseContent.Replace("\"", ""));
            return true;
        }

        if (responseType == typeof(int))
        {
            result = int.Parse(responseContent);
            return true;
        }

        if (responseType == typeof(long))
        {
            result = long.Parse(responseContent);
            return true;
        }

        if (responseType == typeof(decimal))
        {
            result = decimal.Parse(responseContent);
            return true;
        }

        if (responseType == typeof(double))
        {
            result = double.Parse(responseContent);
            return true;
        }

        if (responseType == typeof(Task))
        {
            result = Task.CompletedTask;
            return true;
        }

        return false;
    }

    private static async Task<object?> DeserializeResponse(Type responseType, HttpResponseMessage httpResponseMessage)
    {
        var deserialize = RaitSerializationConfig.DeserializeFunction ??
                          ((content, type) =>
                              content.ReadFromJsonAsync(type, RaitSerializationConfig.SerializationOptions));

        // ActionResult<T>
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ActionResult<>))
        {
            var genericArgument = responseType.GetGenericArguments()[0];
            var genericValue = await deserialize(httpResponseMessage.Content, genericArgument);
            return Activator.CreateInstance(responseType, genericValue);
        }

        // IActionResult non-generic
        if (responseType == typeof(ActionResult) || responseType == typeof(IActionResult))
        {
            return new OkResult();
        }

        // Handle ASP.NET Core typed HTTP results and "Results<...>" unions
        if (TypedResultsHandler.IsTypedHttpResultType(responseType) ||
            TypedResultsHandler.IsResultsUnionType(responseType))
        {
            return await TypedResultsHandler.CreateTypedHttpResult(responseType, httpResponseMessage, deserialize);
        }

        // Fallback: deserialize directly into the requested type
        return await deserialize(httpResponseMessage.Content, responseType);
    }
}
