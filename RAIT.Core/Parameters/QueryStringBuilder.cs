using System.Collections;
using System.Reflection;
using System.Text;

namespace RAIT.Core;

/// <summary>
/// Builds query strings from input parameters using proper URL encoding.
/// </summary>
internal static class QueryStringBuilder
{
    internal static string BuildQueryString(List<InputParameter> parameters)
    {
        var queryParams = parameters
            .Where(p => p is { IsQuery: true, Used: false, Value: not null })
            .Select(p =>
            {
                p.Used = true;
                return FormatParameter(p, p.Name);
            })
            .ToList();

        return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
    }

    private static string FormatParameter(InputParameter parameter, string name)
    {
        var value = parameter.Value!;

        return value switch
        {
            DateTimeOffset dto => FormatValue(name, RaitSerializationConfig.DateTimeOffsetToQuery(dto)),
            DateOnly d => FormatValue(name, RaitSerializationConfig.DateOnlyToQuery(d)),
            DateTime dt => FormatValue(name, RaitSerializationConfig.DateTimeToQuery(dt)),
            string s => $"{name}={s}",
            _ when value.GetType().IsValueType => $"{name}={value}",
            _ => SerializeComplexObject(value)
        };
    }

    private static string FormatValue(string name, string value)
    {
        return $"{name}={Uri.EscapeDataString(value)}";
    }

    private static string SerializeComplexObject(object obj)
    {
        var properties = obj.GetType().GetProperties()
            .Where(prop => prop.CanRead && prop.GetValue(obj) != null)
            .ToDictionary(prop => prop.Name, prop => prop.GetValue(obj)!);

        var parts = new List<string>();

        foreach (var prop in properties)
        {
            if (prop.Value is IEnumerable enumerable && prop.Value is not string)
            {
                parts.AddRange(SerializeEnumerable(prop.Key, enumerable));
            }
            else
            {
                parts.AddRange(SerializeValue(prop.Key, prop.Value));
            }
        }

        return string.Join("&", parts);
    }

    private static IEnumerable<string> SerializeEnumerable(string key, IEnumerable enumerable)
    {
        return enumerable.Cast<object>().Select(value => $"{key}={value}");
    }

    private static IEnumerable<string> SerializeValue(string key, object value)
    {
        var encodedKey = Uri.EscapeDataString(key);

        return value switch
        {
            DateTime dt => new[] { $"{encodedKey}={dt:O}" },
            Guid guid => new[] { $"{encodedKey}={guid}" },
            decimal dec => new[] { $"{encodedKey}={dec}" },
            string s => new[] { $"{encodedKey}={Uri.EscapeDataString(s)}" },
            _ when IsSimpleType(value.GetType()) => new[] { $"{encodedKey}={Uri.EscapeDataString(value.ToString()!)}" },
            _ => SerializeNestedObject(key, value)
        };
    }

    private static IEnumerable<string> SerializeNestedObject(string key, object value)
    {
        return value.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetIndexParameters().Length == 0)
            .Select(p =>
            {
                var stringValue = p.GetValue(value)?.ToString();
                return stringValue != null
                    ? $"{Uri.EscapeDataString(key)}.{p.Name}={Uri.EscapeDataString(stringValue)}"
                    : null;
            })
            .Where(s => s != null)
            .Cast<string>();
    }

    private static bool IsSimpleType(Type type) => type.IsValueType || type == typeof(string);
}
