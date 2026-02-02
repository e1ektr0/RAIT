using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RAIT.Core;

/// <summary>
/// Factory for creating InputParameter objects from method parameters and values.
/// </summary>
internal static class InputParameterFactory
{
    internal static List<InputParameter> CreateFromValue(ParameterInfo parameterInfo, object? value)
    {
        var inputParameters = new List<InputParameter>();

        if (value != null && ShouldExpandProperties(value, parameterInfo))
        {
            inputParameters.AddRange(CreateFromProperties(value));
        }
        else
        {
            inputParameters.Add(CreateBasic(parameterInfo, value));
        }

        return inputParameters;
    }

    private static bool ShouldExpandProperties(object value, ParameterInfo parameterInfo)
    {
        return IsComplexType(value.GetType()) &&
               !HasHttpBindingAttribute(parameterInfo.CustomAttributes) &&
               value.GetType() != typeof(RaitFormFile);
    }

    private static bool IsComplexType(Type type)
    {
        return !type.IsValueType && type != typeof(string) && !type.IsArray;
    }

    private static bool HasHttpBindingAttribute(IEnumerable<CustomAttributeData> attributes)
    {
        return attributes.Any(attr =>
            typeof(FromQueryAttribute).IsAssignableFrom(attr.AttributeType) ||
            typeof(FromFormAttribute).IsAssignableFrom(attr.AttributeType) ||
            typeof(FromHeaderAttribute).IsAssignableFrom(attr.AttributeType) ||
            typeof(FromRouteAttribute).IsAssignableFrom(attr.AttributeType) ||
            typeof(FromBodyAttribute).IsAssignableFrom(attr.AttributeType));
    }

    private static IEnumerable<InputParameter> CreateFromProperties(object value)
    {
        return value.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetIndexParameters().Length == 0)
            .SelectMany(p => CreateFromProperty(p, p.GetValue(value)));
    }

    private static IEnumerable<InputParameter> CreateFromProperty(PropertyInfo property, object? value)
    {
        if (value == null)
            return Enumerable.Empty<InputParameter>();

        var isQuery = HasAttribute<FromQueryAttribute>(property.CustomAttributes);

        if (isQuery && IsComplexType(value.GetType()))
        {
            return CreateNestedQueryParameters(property, value);
        }

        return new[]
        {
            new InputParameter
            {
                Value = value,
                Name = GetParameterName(property) ?? property.Name,
                IsQuery = isQuery,
                IsForm = HasAttribute<FromFormAttribute>(property.CustomAttributes),
                IsBody = HasAttribute<FromBodyAttribute>(property.CustomAttributes),
                IsHeader = HasAttribute<FromHeaderAttribute>(property.CustomAttributes),
                Type = property.PropertyType
            }
        };
    }

    private static IEnumerable<InputParameter> CreateNestedQueryParameters(PropertyInfo property, object value)
    {
        return value.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetIndexParameters().Length == 0)
            .Select(p => new InputParameter
            {
                Value = p.GetValue(value)?.ToString(),
                Name = $"{property.Name}.{GetParameterName(p) ?? p.Name}",
                IsQuery = true,
                Type = p.PropertyType
            });
    }

    private static InputParameter CreateBasic(ParameterInfo parameterInfo, object? value)
    {
        var isSimpleType = parameterInfo.ParameterType.IsValueType || parameterInfo.ParameterType == typeof(string);
        var isQuery = HasAttribute<FromQueryAttribute>(parameterInfo.CustomAttributes) || isSimpleType;

        return new InputParameter
        {
            Value = value,
            Name = GetParameterName(parameterInfo) ?? parameterInfo.Name!,
            IsQuery = isQuery,
            IsForm = HasAttribute<FromFormAttribute>(parameterInfo.CustomAttributes),
            IsHeader = HasAttribute<FromHeaderAttribute>(parameterInfo.CustomAttributes),
            Type = value?.GetType() ?? parameterInfo.ParameterType
        };
    }

    private static bool HasAttribute<TAttribute>(IEnumerable<CustomAttributeData> attributes)
        where TAttribute : Attribute
    {
        return attributes.Any(attr => typeof(TAttribute).IsAssignableFrom(attr.AttributeType));
    }

    private static string? GetParameterName(ParameterInfo parameterInfo)
    {
        var nameAttribute = GetNameProviderAttribute(parameterInfo.GetCustomAttributesData());
        return nameAttribute != null ? ExtractName(nameAttribute) : null;
    }

    private static string? GetParameterName(PropertyInfo propertyInfo)
    {
        var nameAttribute = GetNameProviderAttribute(propertyInfo.GetCustomAttributesData());
        return nameAttribute != null ? ExtractName(nameAttribute) : null;
    }

    private static CustomAttributeData? GetNameProviderAttribute(IEnumerable<CustomAttributeData> attributes)
    {
        return attributes.FirstOrDefault(n => n.AttributeType.GetInterfaces().Contains(typeof(IModelNameProvider)));
    }

    private static string? ExtractName(CustomAttributeData nameAttribute)
    {
        // Try named argument first
        foreach (var namedArg in nameAttribute.NamedArguments)
        {
            if (namedArg.MemberName == "Name")
            {
                return (string?)namedArg.TypedValue.Value;
            }
        }

        // Instantiate attribute to get name from constructor
        var args = nameAttribute.ConstructorArguments.Select(arg => arg.Value).ToArray();
        if (Activator.CreateInstance(nameAttribute.AttributeType, args) is IModelNameProvider attributeInstance)
        {
            return attributeInstance.Name;
        }

        return null;
    }
}
