﻿using System.Collections;

namespace RAIT.Core;

public static class TypeExtension
{
    public static string? ToStringParam<T>(this T value)
    {
        if (value == null)
            return null;
        if (value is not IEnumerable || value is string)
        {
            if (value is Uri)
                return value.ToString();
            if (value.GetType().IsClass && value is not string)
                return null;
            return value.ToString()!;
        }
        var enumerable = (IEnumerable?)value;
        if (enumerable == null)
            return null;
        var type = enumerable.GetType();
        var genericArguments = type.GetGenericArguments();
        var variables = enumerable.Cast<object>().Select(n=>n.ToString()).ToList();
        if (!genericArguments.Any()) 
            return null;
        
        var genericType = genericArguments.FirstOrDefault();
        if(genericType == null)
            return null;
        return !genericType.IsClass ? string.Join(",", variables) : null;
    }
}