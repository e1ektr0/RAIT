using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace RAIT.Core.Http;

/// <summary>
/// Handles creation and mapping of ASP.NET Core typed HTTP results (Ok&lt;T&gt;, Created&lt;T&gt;, etc.)
/// </summary>
internal static class TypedResultsHandler
{
    private static readonly Dictionary<Type, HttpStatusCode> GenericStatusCodes = new()
    {
        { typeof(Ok<>), HttpStatusCode.OK },
        { typeof(Created<>), HttpStatusCode.Created },
        { typeof(Accepted<>), HttpStatusCode.Accepted },
        { typeof(BadRequest<>), HttpStatusCode.BadRequest },
        { typeof(Conflict<>), HttpStatusCode.Conflict },
        { typeof(UnprocessableEntity<>), (HttpStatusCode)422 },
        { typeof(NotFound<>), HttpStatusCode.NotFound },
        { typeof(JsonHttpResult<>), HttpStatusCode.OK }
    };

    private static readonly Dictionary<Type, HttpStatusCode> NonGenericStatusCodes = new()
    {
        { typeof(NotFound), HttpStatusCode.NotFound },
        { typeof(NoContent), HttpStatusCode.NoContent },
        { typeof(BadRequest), HttpStatusCode.BadRequest },
        { typeof(Conflict), HttpStatusCode.Conflict },
        { typeof(UnauthorizedHttpResult), HttpStatusCode.Unauthorized },
        { typeof(UnprocessableEntity), (HttpStatusCode)422 },
        { typeof(ProblemHttpResult), HttpStatusCode.InternalServerError },
        { typeof(Accepted), HttpStatusCode.Accepted }
    };

    internal static bool IsTypedHttpResultType(Type t)
    {
        var ns = t.Namespace ?? string.Empty;
        return ns.Contains("Microsoft.AspNetCore.Http.HttpResults", StringComparison.Ordinal);
    }

    internal static bool IsResultsUnionType(Type t)
    {
        if (!t.IsGenericType) return false;
        var ns = t.Namespace ?? string.Empty;
        return ns.Contains("Microsoft.AspNetCore.Http.HttpResults", StringComparison.Ordinal)
               && t.Name.StartsWith("Results`", StringComparison.Ordinal);
    }

    internal static async Task<object?> CreateTypedHttpResult(
        Type requestedType,
        HttpResponseMessage response,
        Func<HttpContent, Type, Task<object?>> deserialize)
    {
        if (IsResultsUnionType(requestedType))
        {
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

        return await CreateSingleTypedHttpResultInstance(requestedType, response, deserialize);
    }

    private static Type? ChooseUnionMemberForStatus(Type[] candidates, HttpStatusCode status)
    {
        foreach (var c in candidates)
        {
            var expected = GetExpectedStatus(c);
            if (expected.HasValue && expected.Value == status)
                return c;
        }

        // Fallback: any 2xx -> Ok<T> if present
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
            return GenericStatusCodes.TryGetValue(def, out var status) ? status : null;
        }

        return NonGenericStatusCodes.TryGetValue(t, out var nonGenericStatus) ? nonGenericStatus : null;
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

        // Generic types with value
        if (resultType.IsGenericType)
        {
            var def = resultType.GetGenericTypeDefinition();
            var innerType = resultType.GetGenericArguments()[0];
            var value = await DeserializeBodyWithFallback(response, innerType, deserialize);

            if (def == typeof(Ok<>))
                return InvokeTypedResultsGeneric("Ok", innerType, [value]);

            if (def == typeof(Created<>))
            {
                var location = GetLocationHeader(response) ?? "/";
                return InvokeTypedResultsGeneric("Created", innerType, [location, value!]);
            }

            if (def == typeof(Accepted<>))
            {
                var location = GetLocationHeader(response) ?? "/";
                return InvokeTypedResultsGeneric("Accepted", innerType, [location, value!]);
            }

            if (def == typeof(NotFound<>))
                return InvokeTypedResultsGeneric("NotFound", innerType, [value]);

            if (def == typeof(BadRequest<>))
                return InvokeTypedResultsGeneric("BadRequest", innerType, [value]);

            if (def == typeof(Conflict<>))
                return InvokeTypedResultsGeneric("Conflict", innerType, [value]);

            if (def == typeof(UnprocessableEntity<>))
                return InvokeTypedResultsGeneric("UnprocessableEntity", innerType, [value]);

            if (def == typeof(JsonHttpResult<>))
                return InvokeTypedResultsGeneric("Json", innerType, [value, null, null, null]);
        }

        // Non-generic types
        if (resultType == typeof(NotFound) || status == HttpStatusCode.NotFound)
            return InvokeTypedResults("NotFound", Type.EmptyTypes, Array.Empty<object>());

        if (resultType == typeof(BadRequest) || status == HttpStatusCode.BadRequest)
            return InvokeTypedResults("BadRequest", Type.EmptyTypes, Array.Empty<object>());

        if (resultType == typeof(Conflict) || status == HttpStatusCode.Conflict)
            return InvokeTypedResults("Conflict", Type.EmptyTypes, Array.Empty<object>());

        if (resultType == typeof(UnauthorizedHttpResult) || status == HttpStatusCode.Unauthorized)
            return InvokeTypedResults("Unauthorized", Type.EmptyTypes, Array.Empty<object>());

        if (resultType == typeof(UnprocessableEntity) || (int)status == 422)
            return InvokeTypedResults("UnprocessableEntity", Type.EmptyTypes, Array.Empty<object>());

        if (resultType == typeof(ProblemHttpResult))
            return InvokeTypedResults("Problem", Type.EmptyTypes, Array.Empty<object>());

        if (resultType == typeof(NoContent) || status == HttpStatusCode.NoContent)
            return InvokeTypedResults("NoContent", Type.EmptyTypes, Array.Empty<object>());

        if (resultType == typeof(Accepted) || status == HttpStatusCode.Accepted)
        {
            var location = GetLocationHeader(response) ?? "/";
            return InvokeTypedResults("Accepted", Type.EmptyTypes, [location]);
        }

        throw new NotSupportedException(
            $"RAIT: Typed HTTP result '{resultType}' for status {(int)status} {status} is not yet supported.");
    }

    private static string? GetLocationHeader(HttpResponseMessage response)
    {
        if (response.Headers.Location != null)
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

        if (response.Content.Headers.ContentLength == 0)
            return type.IsValueType ? Activator.CreateInstance(type) : null;

        var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
        if (type == typeof(string) && !mediaType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            return await response.Content.ReadAsStringAsync();
        }

        return await deserialize(response.Content, type);
    }

    private static object InvokeTypedResultsGeneric(string methodName, Type genericArg, object?[] args)
    {
        var methods = typeof(TypedResults)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == methodName && m.IsGenericMethodDefinition)
            .Select(m => m.MakeGenericMethod(genericArg))
            .ToList();

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
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
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

    private static bool ParametersCompatible(ParameterInfo[] parameters, object?[] args)
    {
        if (parameters.Length != args.Length) return false;
        for (var i = 0; i < parameters.Length; i++)
        {
            var pType = parameters[i].ParameterType;
            var arg = args[i];
            if (arg == null)
            {
                if (pType.IsValueType && Nullable.GetUnderlyingType(pType) == null)
                    return false;
                continue;
            }

            if (!pType.IsInstanceOfType(arg))
                return false;
        }

        return true;
    }

    private static object ConvertToResultsUnion(Type unionType, object innerInstance)
    {
        var op = unionType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m =>
                m.Name == "op_Implicit" &&
                m.ReturnType == unionType &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType.IsInstanceOfType(innerInstance));

        if (op == null)
        {
            var op2 = innerInstance.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                    m.Name == "op_Implicit" &&
                    m.ReturnType == unionType &&
                    m.GetParameters().Length == 1 &&
                    m.GetParameters()[0].ParameterType.IsInstanceOfType(innerInstance));

            if (op2 == null)
            {
                throw new InvalidOperationException(
                    $"RAIT: Could not find implicit conversion from {innerInstance.GetType()} to {unionType}.");
            }

            return op2.Invoke(null, [innerInstance])!;
        }

        return op.Invoke(null, [innerInstance])!;
    }
}
