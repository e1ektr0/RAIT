using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Net.Http.Headers;

namespace RAIT.Core;

internal static class RaitHttpRequester<TController> where TController : ControllerBase
{
    internal static async Task<TOutput?> HttpRequest<TOutput>(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string route,
        List<InputParameter> prepareInputParameters)
    {
        var memberInfo = typeof(TOutput);
        var result = await ExecuteHttpRequest(httpClient, attributes, route, prepareInputParameters, memberInfo);
        var output = (TOutput?)result;
        DocumentResult(result);
        return output;
    }

    internal static async Task<HttpResponseMessage> HttpRequest(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string route,
        List<InputParameter> prepareInputParameters)
    {
        return await ExecuteHttpRequestInternal(httpClient, attributes, route, prepareInputParameters);
    }

    private static async Task<object?> ExecuteHttpRequest(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string route,
        List<InputParameter> prepareInputParameters, Type memberInfo)
    {
        var customAttributeData = GetHttpMethodAttribute(attributes);
        var httpResponseMessage = await SendHttpRequest(httpClient, customAttributeData, route, prepareInputParameters);

        await HandleError(httpResponseMessage);
        SetCookies(httpClient, httpResponseMessage);

        return await ProcessResponse(memberInfo, httpResponseMessage);
    }

    private static async Task<HttpResponseMessage> ExecuteHttpRequestInternal(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string route,
        List<InputParameter> prepareInputParameters)
    {
        var customAttributeData = GetHttpMethodAttribute(attributes);
        return await SendHttpRequest(httpClient, customAttributeData, route, prepareInputParameters);
    }

    private static CustomAttributeData GetHttpMethodAttribute(IEnumerable<CustomAttributeData> attributes)
    {
        var customAttributeData =
            attributes.FirstOrDefault(n => n.AttributeType.BaseType == typeof(HttpMethodAttribute));
        if (customAttributeData == null)
            throw new Exception("Http type attribute not found");
        return customAttributeData;
    }

    private static async Task<HttpResponseMessage> SendHttpRequest(HttpClient httpClient,
        CustomAttributeData customAttributeData, string route,
        List<InputParameter> prepareInputParameters)
    {
        var httpResponseMessage = customAttributeData.AttributeType switch
        {
            var type when type == typeof(HttpGetAttribute) => await httpClient.GetAsync(route),
            var type when type == typeof(HttpPostAttribute) => await httpClient.PostAsync(route,
                PrepareRequestContent(prepareInputParameters)),
            var type when type == typeof(HttpPutAttribute) => await httpClient.PutAsync(route,
                PrepareRequestContent(prepareInputParameters)),
            var type when type == typeof(HttpPatchAttribute) => await httpClient.PatchAsync(route,
                PrepareRequestContent(prepareInputParameters)),
            var type when type == typeof(HttpDeleteAttribute) => await httpClient.DeleteAsync(route),
            _ => throw new Exception("Rait: Http web attribute not found.")
        };

        return httpResponseMessage;
    }

    private static async Task<object?> ProcessResponse(Type responseType, HttpResponseMessage httpResponseMessage)
    {
        if (httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
            return null;

        var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
        return responseType switch
        {
            var type when type == typeof(EmptyResponse) => null,
            var type when type == typeof(string) || type == typeof(object) => responseContent,
            var type when type == typeof(Guid) => Guid.Parse(responseContent.Replace("\"", "")),
            var type when type == typeof(int) => int.Parse(responseContent),
            var type when type == typeof(long) => long.Parse(responseContent),
            var type when type == typeof(decimal) => decimal.Parse(responseContent),
            var type when type == typeof(double) => double.Parse(responseContent),
            _ => await DeserializeResponse(responseType, httpResponseMessage)
        };
    }

    private static async Task<object?> DeserializeResponse(Type responseType, HttpResponseMessage httpResponseMessage)
    {
        var deserialize = RaitSerializationConfig.DeserializeFunction ??
                          ((x, type) => x.ReadFromJsonAsync(type, RaitSerializationConfig.SerializationOptions));

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ActionResult<>))
        {
            var genericArgument = responseType.GetGenericArguments()[0];
            var genericValue = await deserialize(httpResponseMessage.Content, genericArgument);
            return Activator.CreateInstance(responseType, genericValue);
        }

        return responseType == typeof(ActionResult) || responseType == typeof(IActionResult)
            ? new OkResult()
            : await deserialize(httpResponseMessage.Content, responseType);
    }

    private static HttpContent? PrepareRequestContent(List<InputParameter> prepareInputParameters)
    {
        if (prepareInputParameters.Any(n => n.IsForm || n.Type == typeof(RaitFormFile)))
        {
            return CreateMultipartFormDataContent(prepareInputParameters);
        }

        var generatedInputParameter = prepareInputParameters.FirstOrDefault(n => n.IsForm) ??
                                      prepareInputParameters.FirstOrDefault(n => !n.Used);
        var serializeFunction = RaitSerializationConfig.SerializeFunction ?? (x => JsonContent.Create(x));
        return generatedInputParameter != null ? serializeFunction(generatedInputParameter.Value) : null;
    }

    private static HttpContent CreateMultipartFormDataContent(List<InputParameter> prepareInputParameters)
    {
        var formData = new MultipartFormDataContent();

        foreach (var parameter in prepareInputParameters.Where(n => !n.Used))
        {
            if (parameter.Type == typeof(RaitFormFile))
            {
                AddFormFile(formData, (RaitFormFile)parameter.Value!, parameter.Name);
            }
            else if (parameter.Type!.IsClass)
            {
                AddClassPropertiesToFormData(formData, parameter);
            }
        }

        return formData;
    }

    private static void AddFormFile(MultipartFormDataContent formData, RaitFormFile file, string name)
    {
        var streamContent = new StreamContent(file.OpenReadStream());
        streamContent.Headers.Add("Content-Type", file.ContentType);
        formData.Add(streamContent, name, file.FileName);
    }

    private static void AddClassPropertiesToFormData(MultipartFormDataContent formData, InputParameter parameter)
    {
        foreach (var property in parameter.Type!.GetProperties())
        {
            var value = property.GetValue(parameter.Value);
            switch (value)
            {
                case null:
                    continue;
                case IEnumerable<Guid> listVal:
                {
                    var index = 0;
                    foreach (var v in listVal)
                    {
                        formData.Add(new StringContent(v.ToString()), $"{parameter.Name}.{property.Name}[{index++}]");
                    }

                    break;
                }
                default:
                    formData.Add(new StringContent(value.ToString()!), $"{parameter.Name}.{property.Name}");
                    break;
            }
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