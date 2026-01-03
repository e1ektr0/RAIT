using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Net.Http.Headers;

namespace RAIT.Core;

internal static class RaitHttpRequester<TController> where TController : ControllerBase
{
    internal static async Task<TOutput?> HttpRequest<TOutput>(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string route,
        List<InputParameter> prepareInputParameters, HttpCompletionOption option)
    {
        var memberInfo = typeof(TOutput);
        var result =
            await ExecuteHttpRequest(httpClient, attributes, route, prepareInputParameters, memberInfo, option);
        var output = (TOutput?)result;
        DocumentResult(result);
        return output;
    }

    internal static async Task<HttpResponseMessage> HttpRequest(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string route,
        List<InputParameter> prepareInputParameters, HttpCompletionOption option)
    {
        return await ExecuteHttpRequestInternal(httpClient, attributes, route, prepareInputParameters, option);
    }

    private static async Task<object?> ExecuteHttpRequest(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string route,
        List<InputParameter> prepareInputParameters, Type memberInfo, HttpCompletionOption option)
    {
        var customAttributeData = GetHttpMethodAttribute(attributes);
        var httpResponseMessage =
            await SendHttpRequest(httpClient, customAttributeData, route, prepareInputParameters, option);

        await HandleError(httpResponseMessage);
        SetCookies(httpClient, httpResponseMessage);

        return await ProcessResponse(memberInfo, httpResponseMessage);
    }

    private static async Task<HttpResponseMessage> ExecuteHttpRequestInternal(HttpClient httpClient,
        IEnumerable<CustomAttributeData> attributes, string route,
        List<InputParameter> prepareInputParameters, HttpCompletionOption option)
    {
        var customAttributeData = GetHttpMethodAttribute(attributes);
        return await SendHttpRequest(httpClient, customAttributeData, route, prepareInputParameters, option);
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
        List<InputParameter> prepareInputParameters, HttpCompletionOption option)
    {
        // Prepare the request content only if the attribute isn't HttpGet or HttpDelete.
        HttpContent? prepareRequestContent = null;
        if (customAttributeData.AttributeType != typeof(HttpGetAttribute) &&
            customAttributeData.AttributeType != typeof(HttpDeleteAttribute))
        {
            prepareRequestContent = PrepareRequestContent(prepareInputParameters);
        }

        // Map the attribute type to an HTTP method.
        var method = customAttributeData.AttributeType switch
        {
            { } t when t == typeof(HttpGetAttribute) => HttpMethod.Get,
            { } t when t == typeof(HttpPostAttribute) => HttpMethod.Post,
            { } t when t == typeof(HttpPutAttribute) => HttpMethod.Put,
            { } t when t == typeof(HttpPatchAttribute) => new HttpMethod("PATCH"),
            { } t when t == typeof(HttpDeleteAttribute) => HttpMethod.Delete,
            _ => throw new Exception("Rait: Http web attribute not found.")
        };

        // Create the HttpRequestMessage.
        var request = new HttpRequestMessage(method, route)
        {
            Content = method != HttpMethod.Get && method != HttpMethod.Delete ? prepareRequestContent : null,
        };

        // Add headers to the request from the input parameters.
        foreach (var parameter in prepareInputParameters)
        {
            if (parameter is { IsHeader: true, Value: not null })
            {
                // Use TryAddWithoutValidation to prevent exceptions with invalid header formats.
                request.Headers.TryAddWithoutValidation(parameter.Name, parameter.Value.ToString());
            }
        }

        // Send the request using SendAsync.
        return await httpClient.SendAsync(request, option);
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
            var type when type == typeof(Task) => Task.CompletedTask,
            _ => await DeserializeResponse(responseType, httpResponseMessage)
        };
    }


    private static async Task<object?> DeserializeResponse(Type responseType, HttpResponseMessage httpResponseMessage)
    {
        // Use configured deserializer or default to HttpClient JSON with options
        var deserialize = RaitSerializationConfig.DeserializeFunction ??
                          ((content, type) =>
                              content.ReadFromJsonAsync(type, RaitSerializationConfig.SerializationOptions));

        // ActionResult<T> still handled the old way
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ActionResult<>))
        {
            var genericArgument = responseType.GetGenericArguments()[0];
            var genericValue = await deserialize(httpResponseMessage.Content, genericArgument);
            return Activator.CreateInstance(responseType, genericValue);
        }

        // IActionResult non-generic (we don't know status here; treat success as OkResult)
        if (responseType == typeof(ActionResult) || responseType == typeof(IActionResult))
        {
            return new OkResult();
        }

        // Handle ASP.NET Core typed HTTP results and "Results<...>" unions
        if (IsTypedHttpResultType(responseType) || IsResultsUnionType(responseType))
        {
            return await CreateTypedHttpResult(responseType, httpResponseMessage, deserialize);
        }

        // Fallback: deserialize directly into the requested type
        return await deserialize(httpResponseMessage.Content, responseType);
    }

    private static bool IsTypedHttpResultType(Type t)
    {
        // E.g. Microsoft.AspNetCore.Http.HttpResults.Ok`1, Created`1, NotFound, NotFound`1, etc.
        var ns = t.Namespace ?? string.Empty;
        return ns.Contains("Microsoft.AspNetCore.Http.HttpResults", StringComparison.Ordinal);
    }

    private static bool IsResultsUnionType(Type t)
    {
        // E.g. Microsoft.AspNetCore.Http.HttpResults.Results`2 / `3 / ...
        if (!t.IsGenericType) return false;
        var ns = t.Namespace ?? string.Empty;
        return ns.Contains("Microsoft.AspNetCore.Http.HttpResults", StringComparison.Ordinal)
               && t.Name.StartsWith("Results`", StringComparison.Ordinal);
    }

    private static async Task<object?> CreateTypedHttpResult(
        Type requestedType,
        HttpResponseMessage response,
        Func<HttpContent, Type, Task<object?>> deserialize)
    {
        if (IsResultsUnionType(requestedType))
        {
            // Results<T1, T2, ...> — pick the member matching the status code and convert via implicit operator
            var unionArgs = requestedType.GetGenericArguments();
            var winningMemberType = ChooseUnionMemberForStatus(unionArgs, response.StatusCode);

            if (winningMemberType == null)
            {
                throw new NotSupportedException(
                    $"RAIT: Could not map HTTP {(int)response.StatusCode} {response.StatusCode} " +
                    $"to any member of union {requestedType}.");
            }

            var innerInstance = await CreateSingleTypedHttpResultInstance(winningMemberType, response, deserialize);
            return ConvertToResultsUnion(requestedType, innerInstance);
        }

        // Single typed result (e.g., Created<T>, Ok<T>, NotFound<T>, NotFound, NoContent)
        return await CreateSingleTypedHttpResultInstance(requestedType, response, deserialize);
    }

    private static Type? ChooseUnionMemberForStatus(Type[] candidates, HttpStatusCode status)
    {
        // Map to the first candidate whose expected status matches
        foreach (var c in candidates)
        {
            var expected = GetExpectedStatus(c);
            if (expected.HasValue && expected.Value == status)
                return c;
        }

        // Fallbacks: any 2xx -> Ok<T> if present
        if ((int)status >= 200 && (int)status <= 299)
        {
            var okType = candidates.FirstOrDefault(t => IsGenericTypeDefinition(t, typeof(Ok<>)));
            if (okType != null) return okType;
        }

        return null;
    }

    private static HttpStatusCode? GetExpectedStatus(Type t)
    {
        if (t.IsGenericType)
        {
            var def = t.GetGenericTypeDefinition();

            if (def == typeof(Ok<>)) return HttpStatusCode.OK;
            if (def == typeof(Created<>)) return HttpStatusCode.Created;
            if (def == typeof(Accepted<>)) return HttpStatusCode.Accepted;
            if (def == typeof(BadRequest<>)) return HttpStatusCode.BadRequest;
            if (def == typeof(Conflict<>)) return HttpStatusCode.Conflict;
            if (def == typeof(UnprocessableEntity<>)) return (HttpStatusCode)422;
            // Add others here if you introduce more typed generics
        }
        else
        {
            if (t == typeof(NotFound)) return HttpStatusCode.NotFound;
            if (t == typeof(NoContent)) return HttpStatusCode.NoContent;
            if (t == typeof(BadRequest)) return HttpStatusCode.BadRequest;
            if (t == typeof(Conflict)) return HttpStatusCode.Conflict;
            if (t == typeof(UnauthorizedHttpResult)) return HttpStatusCode.Unauthorized;
            if (t == typeof(UnprocessableEntity)) return (HttpStatusCode)422;
            if (t == typeof(ProblemHttpResult)) return HttpStatusCode.InternalServerError;
            // Add more non-generic cases as needed
        }

        return null;
    }

    private static bool IsGenericTypeDefinition(Type t, Type openGeneric)
    {
        return t.IsGenericType && t.GetGenericTypeDefinition() == openGeneric;
    }

    private static async Task<object> CreateSingleTypedHttpResultInstance(
        Type resultType,
        HttpResponseMessage response,
        Func<HttpContent, Type, Task<object?>> deserialize)
    {
        var status = response.StatusCode;

        // Ok<T>
        if (IsGenericTypeDefinition(resultType, typeof(Ok<>)) ||
            (status == HttpStatusCode.OK && IsGenericTypeDefinition(resultType, typeof(Ok<>))))
        {
            var innerType = resultType.GetGenericArguments()[0];
            var value = await DeserializeBodyWithFallback(response, innerType, deserialize);
            return InvokeTypedResultsGeneric("Ok", innerType, new[] { value });
        }

        // Created<T>
        if (IsGenericTypeDefinition(resultType, typeof(Created<>)) ||
            (status == HttpStatusCode.Created && IsGenericTypeDefinition(resultType, typeof(Created<>))))
        {
            var innerType = resultType.GetGenericArguments()[0];
            var value = await DeserializeBodyWithFallback(response, innerType, deserialize);
            var location = GetLocationHeader(response) ?? "/";
            return InvokeTypedResultsGeneric("Created", innerType, new object[] { location, value! });
        }

        // Accepted<T> or Accepted (non-generic)
        if (IsGenericTypeDefinition(resultType, typeof(Accepted<>)) ||
            status == HttpStatusCode.Accepted && IsGenericTypeDefinition(resultType, typeof(Accepted<>)))
        {
            var innerType = resultType.GetGenericArguments()[0];
            var value = await DeserializeBodyWithFallback(response, innerType, deserialize);
            var location = GetLocationHeader(response) ?? "/";
            return InvokeTypedResultsGeneric("Accepted", innerType, new object[] { location, value! });
        }

        if (resultType == typeof(Accepted) || status == HttpStatusCode.Accepted)
        {
            var location = GetLocationHeader(response) ?? "/";
            return InvokeTypedResults("Accepted", Type.EmptyTypes, new object[] { location });
        }

        // NotFound<T> / NotFound
        if (resultType == typeof(NotFound) || status == HttpStatusCode.NotFound)
        {
            if (resultType.IsGenericType && IsGenericTypeDefinition(resultType, typeof(NotFound<>)))
            {
                var innerType = resultType.GetGenericArguments()[0];
                var value = await DeserializeBodyWithFallback(response, innerType, deserialize);
                return InvokeTypedResultsGeneric("NotFound", innerType, new[] { value });
            }

            return InvokeTypedResults("NotFound", Type.EmptyTypes, Array.Empty<object>());
        }

        // BadRequest<T> / BadRequest
        if (IsGenericTypeDefinition(resultType, typeof(BadRequest<>)) ||
            status == HttpStatusCode.BadRequest && IsGenericTypeDefinition(resultType, typeof(BadRequest<>)))
        {
            var innerType = resultType.GetGenericArguments()[0];
            var value = await DeserializeBodyWithFallback(response, innerType, deserialize);
            return InvokeTypedResultsGeneric("BadRequest", innerType, new[] { value });
        }

        if (resultType == typeof(BadRequest) || status == HttpStatusCode.BadRequest)
        {
            // Pass no args => BadRequest()
            return InvokeTypedResults("BadRequest", Type.EmptyTypes, Array.Empty<object>());
        }

        // Conflict<T> / Conflict
        if (IsGenericTypeDefinition(resultType, typeof(Conflict<>)) ||
            status == HttpStatusCode.Conflict && IsGenericTypeDefinition(resultType, typeof(Conflict<>)))
        {
            var innerType = resultType.GetGenericArguments()[0];
            var value = await DeserializeBodyWithFallback(response, innerType, deserialize);
            return InvokeTypedResultsGeneric("Conflict", innerType, new[] { value });
        }

        if (resultType == typeof(Conflict) || status == HttpStatusCode.Conflict)
        {
            return InvokeTypedResults("Conflict", Type.EmptyTypes, Array.Empty<object>());
        }

        // Unauthorized
        if (resultType == typeof(UnauthorizedHttpResult) || status == HttpStatusCode.Unauthorized)
        {
            return InvokeTypedResults("Unauthorized", Type.EmptyTypes, Array.Empty<object>());
        }

        // UnprocessableEntity<T> / UnprocessableEntity
        if (IsGenericTypeDefinition(resultType, typeof(UnprocessableEntity<>)) ||
            (int)status == 422 && IsGenericTypeDefinition(resultType, typeof(UnprocessableEntity<>)))
        {
            var innerType = resultType.GetGenericArguments()[0];
            var value = await DeserializeBodyWithFallback(response, innerType, deserialize);
            return InvokeTypedResultsGeneric("UnprocessableEntity", innerType, new[] { value });
        }

        if (resultType == typeof(UnprocessableEntity) || (int)status == 422)
        {
            return InvokeTypedResults("UnprocessableEntity", Type.EmptyTypes, Array.Empty<object>());
        }

        if (resultType == typeof(ProblemHttpResult))
        {
            // Use the default Problem() factory (status 500 unless specified)
            return InvokeTypedResults("Problem", Type.EmptyTypes, Array.Empty<object>());
        }

        // NoContent
        if (resultType == typeof(NoContent) || status == HttpStatusCode.NoContent)
        {
            return InvokeTypedResults("NoContent", Type.EmptyTypes, Array.Empty<object>());
        }

        throw new NotSupportedException(
            $"RAIT: Typed HTTP result '{resultType}' for status {(int)status} {status} is not yet supported.");
    }

    private static string? GetLocationHeader(HttpResponseMessage response)
    {
        if (response.Headers?.Location != null)
            return response.Headers.Location.ToString();

        if (response.Headers.TryGetValues("Location", out var values))
            return values.FirstOrDefault();

        return null;
    }

    private static async Task<object?> DeserializeBodyWithFallback(
        HttpResponseMessage response,
        Type type,
        Func<HttpContent, Type, Task<object?>> deserialize)
    {
        if (response.Content == null)
            return type.IsValueType ? Activator.CreateInstance(type) : null;

        // If no content (0 bytes), just return default
        if (response.Content.Headers?.ContentLength == 0)
            return type.IsValueType ? Activator.CreateInstance(type) : null;

        // Special-case string: often NotFound<string> uses text/plain, not JSON
        var mediaType = response.Content.Headers?.ContentType?.MediaType ?? string.Empty;
        if (type == typeof(string) && !mediaType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            return await response.Content.ReadAsStringAsync();
        }

        return await deserialize(response.Content, type);
    }

    private static object InvokeTypedResultsGeneric(string methodName, Type genericArg, object[] args)
    {
        // Find TypedResults.<methodName><T>(...)
        var methods = typeof(TypedResults)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(m => m.Name == methodName && m.IsGenericMethodDefinition)
            .Select(m => m.MakeGenericMethod(genericArg))
            .ToList();

        // Choose overload based on parameter compatibility
        foreach (var m in methods)
        {
            var pars = m.GetParameters();
            if (ParametersCompatible(pars, args))
            {
                return m.Invoke(null, args)!;
            }
        }

        throw new MissingMethodException(
            $"RAIT: Could not find TypedResults.{methodName}<{genericArg.Name}> overload for provided arguments.");
    }

    private static object InvokeTypedResults(string methodName, Type[] genericArgs, object[] args)
    {
        var candidates = typeof(TypedResults)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(m => m.Name == methodName);

        foreach (var m in candidates)
        {
            var mi = m;
            if (m.IsGenericMethodDefinition && genericArgs.Length > 0)
            {
                if (m.GetGenericArguments().Length != genericArgs.Length) continue;
                mi = m.MakeGenericMethod(genericArgs);
            }

            var pars = mi.GetParameters();
            if (ParametersCompatible(pars, args))
            {
                return mi.Invoke(null, args)!;
            }
        }

        throw new MissingMethodException(
            $"RAIT: Could not find TypedResults.{methodName} overload for provided arguments.");
    }

    private static bool ParametersCompatible(System.Reflection.ParameterInfo[] parameters, object[] args)
    {
        if (parameters.Length != args.Length) return false;
        for (int i = 0; i < parameters.Length; i++)
        {
            var pType = parameters[i].ParameterType;
            var arg = args[i];
            if (arg == null)
            {
                if (pType.IsValueType && Nullable.GetUnderlyingType(pType) == null)
                    return false;
                continue;
            }

            if (!pType.IsAssignableFrom(arg.GetType()))
                return false;
        }

        return true;
    }

    private static object ConvertToResultsUnion(Type unionType, object innerInstance)
    {
        // Use implicit conversion operator: public static implicit operator Results<...>(Ok<T> value) ...
        // It is defined on the Results<...> type.
        var op = unionType
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .FirstOrDefault(m =>
                m.Name == "op_Implicit" &&
                m.ReturnType == unionType &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType.IsAssignableFrom(innerInstance.GetType()));

        if (op == null)
        {
            // As a fallback, also check on the inner type (some runtimes place it there)
            var op2 = innerInstance.GetType()
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .FirstOrDefault(m =>
                    m.Name == "op_Implicit" &&
                    m.ReturnType == unionType &&
                    m.GetParameters().Length == 1 &&
                    m.GetParameters()[0].ParameterType.IsAssignableFrom(innerInstance.GetType()));

            if (op2 == null)
            {
                throw new InvalidOperationException(
                    $"RAIT: Could not find implicit conversion from {innerInstance.GetType()} to {unionType}.");
            }

            return op2.Invoke(null, new[] { innerInstance })!;
        }

        return op.Invoke(null, new[] { innerInstance })!;
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
        if (parameter.Type == typeof(string) && parameter.Value is not null)
        {
            formData.Add(new StringContent(parameter.Value.ToString()!), $"{parameter.Name}");
        }
        else
        {
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
                    {
                        var index = 0;
                        foreach (var v in listVal)
                        {
                            formData.Add(new StringContent(v.ToString()),
                                $"{parameter.Name}.{property.Name}[{index++}]");
                        }

                        break;
                    }
                    default:
                        formData.Add(new StringContent(value.ToString()!), $"{parameter.Name}.{property.Name}");
                        break;
                }
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