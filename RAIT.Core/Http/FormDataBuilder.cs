using System.Net.Http.Json;

namespace RAIT.Core;

/// <summary>
/// Builds HTTP content for requests, including multipart form data and JSON content.
/// </summary>
internal static class FormDataBuilder
{
    internal static HttpContent? PrepareRequestContent(List<InputParameter> prepareInputParameters)
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
        if (parameter.Type == typeof(string) && parameter.Value is not null)
        {
            formData.Add(new StringContent(parameter.Value.ToString()!), parameter.Name);
            return;
        }

        foreach (var property in parameter.Type!.GetProperties())
        {
            if (parameter.Value == null)
                continue;

            var value = property.GetValue(parameter.Value);
            switch (value)
            {
                case null:
                    continue;
                case IEnumerable<Guid> listVal:
                    AddGuidEnumerable(formData, parameter.Name, property.Name, listVal);
                    break;
                default:
                    formData.Add(new StringContent(value.ToString()!), $"{parameter.Name}.{property.Name}");
                    break;
            }
        }
    }

    private static void AddGuidEnumerable(MultipartFormDataContent formData, string paramName, string propName, IEnumerable<Guid> values)
    {
        var index = 0;
        foreach (var v in values)
        {
            formData.Add(new StringContent(v.ToString()), $"{paramName}.{propName}[{index++}]");
        }
    }
}
