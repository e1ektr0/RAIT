using System.Net.Http.Json;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace RAIT.Core;

internal static class RaitHttpRequester
{
    internal static async Task<TOutput?> HttpRequest<TOutput>(HttpClient httpClient,
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
            await HandleError(httpResponseMessage);
            httpResponseMessage.EnsureSuccessStatusCode();
            return await httpResponseMessage.Content.ReadFromJsonAsync<TOutput>();
        }

        if (customAttributeData.AttributeType == typeof(HttpPostAttribute))
        {
            var generatedInputParameter = prepareInputParameters.FirstOrDefault(n => !n.Used);
            var jsonContent = JsonContent.Create(generatedInputParameter?.Value);
            var httpResponseMessage = await httpClient.PostAsync(rout, jsonContent);
            await HandleError(httpResponseMessage);
            httpResponseMessage.EnsureSuccessStatusCode();
            return await httpResponseMessage.Content.ReadFromJsonAsync<TOutput>();
        }

        if (customAttributeData.AttributeType == typeof(HttpPutAttribute))
        {
            var generatedInputParameter = prepareInputParameters.FirstOrDefault(n => !n.Used);
            var jsonContent = JsonContent.Create(generatedInputParameter?.Value);
            if (generatedInputParameter == null)
                jsonContent = null;
            var httpResponseMessage = await httpClient.PutAsync(rout, jsonContent);
            await HandleError(httpResponseMessage);
            httpResponseMessage.EnsureSuccessStatusCode();
            return await httpResponseMessage.Content.ReadFromJsonAsync<TOutput>();
        }

        if (customAttributeData.AttributeType == typeof(HttpDeleteAttribute))
        {
            var httpResponseMessage = await httpClient.DeleteAsync(rout);
            await HandleError(httpResponseMessage);
            httpResponseMessage.EnsureSuccessStatusCode();
            return await httpResponseMessage.Content.ReadFromJsonAsync<TOutput>();
        }

        throw new Exception("wtf");
    }

    private static async Task HandleError(HttpResponseMessage httpResponseMessage)
    {
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            var readAsStringAsync = await httpResponseMessage.Content.ReadAsStringAsync();
            throw new Exception(readAsStringAsync);
        }
    }
}