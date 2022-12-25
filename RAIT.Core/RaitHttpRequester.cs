using System.Net.Http.Json;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace RAIT.Core;

public class RaitFormFile : IFormFile
{
    public RaitFormFile(string name, string contentType)
    {
        Name = name;
        FileName = name;
        ContentType = contentType;
    }

    public Stream OpenReadStream()
    {
        return File.Open(Name, FileMode.Open);
    }

    public void CopyTo(Stream target)
    {
        throw new NotImplementedException();
    }

    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    public string ContentType { get; }
    public string? ContentDisposition { get; } = null;
    public IHeaderDictionary Headers { get; } = new HeaderDictionary();
    public long Length { get; } = 0;
    public string Name { get; }
    public string FileName { get; }
}

internal static class RaitHttpRequester
{
    internal static async Task<TOutput?> HttpRequest<TOutput>(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string rout,
        List<InputParameter> prepareInputParameters) where TOutput : class
    {
        var customAttributeData =
            attributes.FirstOrDefault(n => n.AttributeType.BaseType == typeof(HttpMethodAttribute));
        if (customAttributeData == null)
            throw new Exception("Http type attribute not found");
        HttpResponseMessage? httpResponseMessage = null;
        if (customAttributeData.AttributeType == typeof(HttpGetAttribute))
        {
            httpResponseMessage = await httpClient.GetAsync(rout);
            return await httpResponseMessage.Content.ReadFromJsonAsync<TOutput>();
        }

        if (customAttributeData.AttributeType == typeof(HttpPostAttribute))
        {
            var httpContent = PrepareRequestContent(prepareInputParameters);
            httpResponseMessage = await httpClient.PostAsync(rout, httpContent);
        }

        if (customAttributeData.AttributeType == typeof(HttpPutAttribute))
        {
            var httpContent = PrepareRequestContent(prepareInputParameters);
            httpResponseMessage = await httpClient.PutAsync(rout, httpContent);
        }

        if (customAttributeData.AttributeType == typeof(HttpDeleteAttribute))
        {
            httpResponseMessage = await httpClient.DeleteAsync(rout);
        }

        if (httpResponseMessage == null)
            throw new Exception("Rait: Http web attribute not found.");

        await HandleError(httpResponseMessage);
        return await httpResponseMessage.Content.ReadFromJsonAsync<TOutput>();
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