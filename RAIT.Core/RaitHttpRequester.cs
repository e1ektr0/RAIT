﻿using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Net.Http.Headers;

namespace RAIT.Core;

internal static class RaitHttpRequester
{
    internal static async Task<TOutput?> HttpRequest<TOutput>(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string route,
        List<InputParameter> prepareInputParameters)
    {
        var memberInfo = typeof(TOutput);
        var result = await HttpRequest(httpClient, attributes, route, prepareInputParameters, memberInfo);
        return (TOutput?)result;
    }

    internal static async Task<HttpResponseMessage> HttpRequestH(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string route,
        List<InputParameter> prepareInputParameters)
    {
        var result = await HttpRequestHI(httpClient, attributes, route, prepareInputParameters);
        return result;
    }

    internal static async Task<object?> HttpRequest(HttpClient httpClient, IEnumerable<CustomAttributeData> attributes,
        string route,
        List<InputParameter> prepareInputParameters, Type memberInfo)
    {
        object? result;
        var customAttributeData =
            attributes.FirstOrDefault(n => n.AttributeType.BaseType == typeof(HttpMethodAttribute));
        if (customAttributeData == null)
            throw new Exception("Http type attribute not found");
        HttpResponseMessage? httpResponseMessage = null;
        if (customAttributeData.AttributeType == typeof(HttpGetAttribute))
        {
            httpResponseMessage = await httpClient.GetAsync(route);

            await HandleError(httpResponseMessage);
            SetCookies(httpClient, httpResponseMessage);

            if (httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
                result = null!;
            else if (memberInfo == typeof(IActionResult))
                result = new StatusCodeResult((int)httpResponseMessage.StatusCode);
            else
                result = await ProcessHttpResult(memberInfo, httpResponseMessage);
        }
        else
        {
            if (customAttributeData.AttributeType == typeof(HttpPostAttribute))
            {
                var httpContent = PrepareRequestContent(prepareInputParameters);
                httpResponseMessage = await httpClient.PostAsync(route, httpContent);
            }
            else if (customAttributeData.AttributeType == typeof(HttpPutAttribute))
            {
                var httpContent = PrepareRequestContent(prepareInputParameters);
                httpResponseMessage = await httpClient.PutAsync(route, httpContent);
            }
            else if (customAttributeData.AttributeType == typeof(HttpPatchAttribute))
            {
                var httpContent = PrepareRequestContent(prepareInputParameters);
                httpResponseMessage = await httpClient.PatchAsync(route, httpContent);
            }
            else if (customAttributeData.AttributeType == typeof(HttpDeleteAttribute))
            {
                httpResponseMessage = await httpClient.DeleteAsync(route);
            }
            else if (httpResponseMessage == null)
                throw new Exception("Rait: Http web attribute not found.");

            await HandleError(httpResponseMessage);
            SetCookies(httpClient, httpResponseMessage);

            if (httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
                result = null;
            else if (memberInfo == typeof(IActionResult))
                result = new StatusCodeResult((int)httpResponseMessage.StatusCode);
            else
            {
                result = await ProcessHttpResult(memberInfo, httpResponseMessage);
            }
        }

        return result;
    }

    internal static async Task<HttpResponseMessage> HttpRequestHI(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes,
        string route,
        List<InputParameter> prepareInputParameters)
    {
        var customAttributeData =
            attributes.FirstOrDefault(n => n.AttributeType.BaseType == typeof(HttpMethodAttribute));
        if (customAttributeData == null)
            throw new Exception("Http type attribute not found");
        HttpResponseMessage? httpResponseMessage = null;
        if (customAttributeData.AttributeType == typeof(HttpGetAttribute))
        {
            httpResponseMessage = await httpClient.GetAsync(route);
            return httpResponseMessage;
        }

        if (customAttributeData.AttributeType == typeof(HttpPostAttribute))
        {
            var httpContent = PrepareRequestContent(prepareInputParameters);
            httpResponseMessage = await httpClient.PostAsync(route, httpContent);
        }
        else if (customAttributeData.AttributeType == typeof(HttpPutAttribute))
        {
            var httpContent = PrepareRequestContent(prepareInputParameters);
            httpResponseMessage = await httpClient.PutAsync(route, httpContent);
        }
        else if (customAttributeData.AttributeType == typeof(HttpPatchAttribute))
        {
            var httpContent = PrepareRequestContent(prepareInputParameters);
            httpResponseMessage = await httpClient.PatchAsync(route, httpContent);
        }
        else if (customAttributeData.AttributeType == typeof(HttpDeleteAttribute))
        {
            httpResponseMessage = await httpClient.DeleteAsync(route);
        }
        else if (httpResponseMessage == null)
            throw new Exception("Rait: Http web attribute not found.");

        return httpResponseMessage;
    }

    internal static async Task<string> HttpRequestWithoutDeserialization(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes,
        string route, List<InputParameter> prepareInputParameters)
    {
        var customAttributeData =
            attributes.FirstOrDefault(n => n.AttributeType.BaseType == typeof(HttpMethodAttribute));
        if (customAttributeData == null)
            throw new Exception("Http type attribute not found");
        HttpResponseMessage? httpResponseMessage = null;
        if (customAttributeData.AttributeType == typeof(HttpGetAttribute))
            httpResponseMessage = await httpClient.GetAsync(route);
        else
        {
            if (customAttributeData.AttributeType == typeof(HttpPostAttribute))
            {
                var httpContent = PrepareRequestContent(prepareInputParameters);
                httpResponseMessage = await httpClient.PostAsync(route, httpContent);
            }
            else if (customAttributeData.AttributeType == typeof(HttpPutAttribute))
            {
                var httpContent = PrepareRequestContent(prepareInputParameters);
                httpResponseMessage = await httpClient.PutAsync(route, httpContent);
            }
            else if (customAttributeData.AttributeType == typeof(HttpPatchAttribute))
            {
                var httpContent = PrepareRequestContent(prepareInputParameters);
                httpResponseMessage = await httpClient.PatchAsync(route, httpContent);
            }
            else if (customAttributeData.AttributeType == typeof(HttpDeleteAttribute))
            {
                httpResponseMessage = await httpClient.DeleteAsync(route);
            }
            else if (httpResponseMessage == null)
                throw new Exception("Rait: Http web attribute not found.");

            await HandleError(httpResponseMessage);
            SetCookies(httpClient, httpResponseMessage);
        }

        return await httpResponseMessage.Content.ReadAsStringAsync();
    }

    private static async Task<object?> ProcessHttpResult(Type responseType,
        HttpResponseMessage httpResponseMessage)
    {
        try
        {
            var response = await httpResponseMessage.Content.ReadAsStringAsync();
            object? result;
            if (responseType == typeof(EmptyResponse))
                return null;

            //todo: extend
            if (responseType == typeof(object))
                result = response;
            else if (responseType == typeof(string))
                result = response;
            else if (responseType == typeof(Guid))
                result = Guid.Parse(response.Replace("\"", ""));
            else if (responseType == typeof(int))
                result = int.Parse(response);
            else if (responseType == typeof(long))
                result = long.Parse(response);
            else if (responseType == typeof(decimal))
                result = decimal.Parse(response);
            else if (responseType == typeof(double))
                result = double.Parse(response);
            else
            {
                var deserialize = RaitConfig.DeserializeFunction ??
                                  ((x, type) => x.ReadFromJsonAsync(type,
                                      RaitConfig.SerializationOptions));

                if (responseType.IsGenericType)
                {
                    var genericTypeDefinition = responseType.GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(ActionResult<>))
                    {
                        var genericArguments = responseType.GetGenericArguments();
                        var genericArgument = genericArguments[0];
                        var genericValue = await deserialize(httpResponseMessage.Content, genericArgument);
                        result = Activator.CreateInstance(responseType, genericValue);
                        return result;
                    }
                }

                if (responseType == typeof(ActionResult))
                {
                    return new OkResult();
                }

                result = await deserialize(httpResponseMessage.Content, responseType);
            }

            return result;
        }
        catch (Exception e)
        {
            var readAsStringAsync = await httpResponseMessage.Content.ReadAsStringAsync();
            throw new Exception("Fail deserialize:" + readAsStringAsync, e);
        }
    }

    private static HttpContent? PrepareRequestContent(List<InputParameter> prepareInputParameters)
    {
        if (prepareInputParameters.Any(n => n.IsForm || n.Type == typeof(RaitFormFile)))
        {
            // todo: serialize model to form data:
            // HttpContent stringContent = new StringContent(paramString);
            // HttpContent fileStreamContent = new StreamContent(paramFileStream);
            // HttpContent bytesContent = new ByteArrayContent(paramFileBytes);
            var formData = new MultipartFormDataContent();
            foreach (var parameter in prepareInputParameters.Where(n => !n.Used))
            {
                if (parameter.Type == typeof(RaitFormFile))
                {
                    var parameterValue = (RaitFormFile)parameter.Value!;
                    var streamContent = new StreamContent(parameterValue.OpenReadStream());
                    streamContent.Headers.Add("Content-Type", parameterValue.ContentType);
                    formData.Add(streamContent, parameter.Name,
                        parameterValue.FileName);
                }
                else if (parameter.Type!.IsClass)
                {
                    foreach (var p in parameter.Type!.GetProperties())
                    {
                        var val = p.GetValue(parameter.Value);
                        switch (val)
                        {
                            case null:
                                continue;
                            case IEnumerable<Guid> listVal:
                            {
                                var index = 0;
                                foreach (var v in listVal)
                                    formData.Add(new StringContent(v.ToString()),
                                        $"{parameter.Name}.{p.Name}[{index++}]");
                                continue;
                            }
                            default:
                                formData.Add(new StringContent(p.GetValue(parameter.Value!)!.ToString()!),
                                    $"{parameter.Name}.{p.Name}");
                                break;
                        }
                    }
                }
            }

            return formData;
        }

        var generatedInputParameter = prepareInputParameters.FirstOrDefault(n => n.IsForm) ??
                                      prepareInputParameters.FirstOrDefault(n => !n.Used);
        var serializeFunction = RaitConfig.SerializeFunction
                                ?? (x => JsonContent.Create(x));
        var jsonContent = serializeFunction(generatedInputParameter?.Value);
        if (generatedInputParameter == null)
            jsonContent = null;
        return jsonContent;
    }

    private static async Task HandleError(HttpResponseMessage httpResponseMessage)
    {
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            var readAsStringAsync = await httpResponseMessage.Content.ReadAsStringAsync();
            throw new RaitHttpException(readAsStringAsync, httpResponseMessage.StatusCode);
        }
    }

    private static void SetCookies(HttpClient httpClient, HttpResponseMessage httpResponseMessage)
    {
        if (httpResponseMessage.Headers.TryGetValues(HeaderNames.SetCookie, out var cookies))
            httpClient.DefaultRequestHeaders.Add("Cookie", cookies);
    }
}