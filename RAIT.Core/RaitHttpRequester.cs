using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Net.Http.Headers;

namespace RAIT.Core;

internal static class RaitHttpRequester<TController> where TController : ControllerBase
{
    private static readonly Dictionary<Type, HttpMethod> HttpMethodMap = new()
    {
        { typeof(HttpGetAttribute), HttpMethod.Get },
        { typeof(HttpPostAttribute), HttpMethod.Post },
        { typeof(HttpPutAttribute), HttpMethod.Put },
        { typeof(HttpPatchAttribute), new HttpMethod("PATCH") },
        { typeof(HttpDeleteAttribute), HttpMethod.Delete }
    };

    internal static async Task<TOutput?> HttpRequest<TOutput>(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string route,
        List<InputParameter> prepareInputParameters, HttpCompletionOption option)
    {
        var result = await ExecuteHttpRequest(httpClient, attributes, route, prepareInputParameters, typeof(TOutput), option);
        var output = (TOutput?)result;
        DocumentResult(result);
        return output;
    }

    internal static async Task<HttpResponseMessage> HttpRequest(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string route,
        List<InputParameter> prepareInputParameters, HttpCompletionOption option)
    {
        var customAttributeData = GetHttpMethodAttribute(attributes);
        return await SendHttpRequest(httpClient, customAttributeData, route, prepareInputParameters, option);
    }

    private static async Task<object?> ExecuteHttpRequest(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string route,
        List<InputParameter> prepareInputParameters, Type memberInfo, HttpCompletionOption option)
    {
        var customAttributeData = GetHttpMethodAttribute(attributes);
        var httpResponseMessage = await SendHttpRequest(httpClient, customAttributeData, route, prepareInputParameters, option);

        await HandleError(httpResponseMessage);
        SetCookies(httpClient, httpResponseMessage);

        return await ResponseProcessor.ProcessResponse(memberInfo, httpResponseMessage);
    }

    private static CustomAttributeData GetHttpMethodAttribute(IEnumerable<CustomAttributeData> attributes)
    {
        var customAttributeData = attributes.FirstOrDefault(n => n.AttributeType.BaseType == typeof(HttpMethodAttribute));
        if (customAttributeData == null)
            throw new Exception("Http type attribute not found");
        return customAttributeData;
    }

    private static async Task<HttpResponseMessage> SendHttpRequest(HttpClient httpClient,
        CustomAttributeData customAttributeData, string route,
        List<InputParameter> prepareInputParameters, HttpCompletionOption option)
    {
        var method = GetHttpMethod(customAttributeData.AttributeType);
        var hasBody = method != HttpMethod.Get && method != HttpMethod.Delete;

        HttpContent? prepareRequestContent = null;
        if (hasBody)
        {
            prepareRequestContent = FormDataBuilder.PrepareRequestContent(prepareInputParameters);
        }

        var request = new HttpRequestMessage(method, route)
        {
            Content = hasBody ? prepareRequestContent : null,
        };

        AddHeadersToRequest(request, prepareInputParameters);

        return await httpClient.SendAsync(request, option);
    }

    private static HttpMethod GetHttpMethod(Type attributeType)
    {
        if (HttpMethodMap.TryGetValue(attributeType, out var method))
            return method;
        throw new Exception("Rait: Http web attribute not found.");
    }

    private static void AddHeadersToRequest(HttpRequestMessage request, List<InputParameter> parameters)
    {
        foreach (var parameter in parameters.Where(p => p is { IsHeader: true, Value: not null }))
        {
            request.Headers.TryAddWithoutValidation(parameter.Name, parameter.Value!.ToString());
        }
    }

    private static async Task HandleError(HttpResponseMessage httpResponseMessage)
    {
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            var errorContent = await httpResponseMessage.Content.ReadAsStringAsync();
            throw new RaitHttpException(errorContent, httpResponseMessage.StatusCode);
        }
    }

    private static void SetCookies(HttpClient httpClient, HttpResponseMessage httpResponseMessage)
    {
        if (httpResponseMessage.Headers.TryGetValues(HeaderNames.SetCookie, out var cookies))
        {
            httpClient.DefaultRequestHeaders.Add("Cookie", cookies);
        }
    }

    private static void DocumentResult<TOutput>(TOutput? result)
    {
        if (result != null)
        {
            RaitDocumentationGenerator.Params<TController>(new List<InputParameter>
            {
                new() { Value = result, Type = result.GetType() }
            });
        }
    }
}
