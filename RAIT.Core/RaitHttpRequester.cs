using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

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

    private static async Task<object?> ProcessHttpResult(Type memberInfo,
        HttpResponseMessage httpResponseMessage)
    {
        try
        {
            var response = await httpResponseMessage.Content.ReadAsStringAsync();
            object? result;
            if (memberInfo == typeof(EmptyResponse))
                return null;

            //todo: extend
            if (memberInfo == typeof(object))
                result = response;
            else if (memberInfo == typeof(string))
                result = response;
            else if (memberInfo == typeof(Guid))
                result = Guid.Parse(response.Replace("\"", ""));
            else if (memberInfo == typeof(int))
                result = int.Parse(response);
            else if (memberInfo == typeof(long))
                result = long.Parse(response);
            else if (memberInfo == typeof(decimal))
                result = decimal.Parse(response);
            else if (memberInfo == typeof(double))
                result = double.Parse(response);
            else
                result = await httpResponseMessage.Content.ReadFromJsonAsync(memberInfo,
                    RaitConfig.SerializationOptions);
            return result;
        }
        catch (Exception e)
        {
            throw new Exception("Fail deserialize:" + httpResponseMessage.Content.ReadAsStringAsync(), e);
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
                        formData.Add(new StringContent(p.GetValue(parameter.Value!)!.ToString()!),
                            $"{parameter.Name}.{p.Name}");
                }
            }

            return formData;
        }

        var generatedInputParameter = prepareInputParameters.FirstOrDefault(n => !n.Used);
        var jsonContent = JsonContent.Create(generatedInputParameter?.Value);
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
}