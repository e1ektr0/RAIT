using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace RAIT.Core;

/// <summary>
/// Processes route templates by substituting parameter values and removing constraints.
/// </summary>
internal static partial class RouteTemplateProcessor
{
    // Matches route constraints like {id:guid}, {id:int}, {id:long}, {id:string}
    [GeneratedRegex(@"\{(\w+):\w+\}", RegexOptions.Compiled)]
    private static partial Regex RouteConstraintRegex();

    internal static string ProcessRoute(MemberInfo memberInfo, IList<CustomAttributeData> attributes,
        List<InputParameter> inputParameters)
    {
        var routeTemplate = GetRouteTemplate(attributes);
        if (routeTemplate == null) return string.Empty;

        var route = routeTemplate.Replace("[controller]", memberInfo.Name.Replace("Controller", ""));
        route = SubstituteParameters(route, inputParameters);

        return route;
    }

    private static string SubstituteParameters(string route, List<InputParameter> inputParameters)
    {
        // Remove route constraints like :guid, :int, :long, :string
        var processedRoute = RouteConstraintRegex().Replace(route, "{$1}");

        foreach (var param in inputParameters.Where(p => p.Value != null))
        {
            var placeholder = $"{{{param.Name}}}";
            if (processedRoute.Contains(placeholder))
            {
                processedRoute = processedRoute.Replace(placeholder, param.Value!.ToString());
                param.Used = true;
            }
        }

        return processedRoute;
    }

    internal static string? GetRouteTemplate(IList<CustomAttributeData> attributes)
    {
        var routeAttribute = attributes.FirstOrDefault(attr => attr.AttributeType == typeof(RouteAttribute));
        var httpMethodAttribute = attributes.FirstOrDefault(attr => attr.AttributeType.BaseType == typeof(HttpMethodAttribute));

        return GetRouteFromAttribute(routeAttribute) ?? GetRouteFromAttribute(httpMethodAttribute);
    }

    private static string? GetRouteFromAttribute(CustomAttributeData? attributeData)
    {
        return attributeData?.ConstructorArguments.FirstOrDefault().Value as string;
    }
}
