using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace RAIT.Core
{
    internal static class RaitRouter
    {
        internal static string PrepareRoute<TController, TOutput>(
            Expression<Func<TController, Task<TOutput>>> expression, List<InputParameter> inputParameters)
        {
            var methodCall = expression.Body as MethodCallExpression;
            return GenerateRoute<TController>(inputParameters, methodCall);
        }

        internal static string PrepareRoute<TController, TOutput>(
            Expression<Func<TController, TOutput>> expression, List<InputParameter> inputParameters)
        {
            var methodCall = expression.Body as MethodCallExpression;
            return GenerateRoute<TController>(inputParameters, methodCall);
        }

        internal static string PrepareRoute<TController>(
            Expression<Func<TController>> expression, List<InputParameter> inputParameters)
        {
            var methodCall = expression.Body as MethodCallExpression;
            return GenerateRoute<TController>(inputParameters, methodCall);
        }

        internal static string PrepareRoute<TController>(Expression<Func<TController, Task>> expression,
            List<InputParameter> inputParameters)
        {
            var methodCall = expression.Body as MethodCallExpression;
            return GenerateRoute<TController>(inputParameters, methodCall);
        }

        private static string GenerateRoute<TController>(List<InputParameter> inputParameters,
            MethodCallExpression? methodCall)
        {
            if (methodCall == null) throw new ArgumentNullException(nameof(methodCall));

            var methodInfo = methodCall.Method;
            var controllerType = typeof(TController);
            methodInfo = controllerType.GetMethod(methodInfo.Name);
            var routeAttributes = controllerType.GetCustomAttributesData();
            var methodAttributes = methodInfo!.GetCustomAttributesData();

            var route = BuildRoute(controllerType, routeAttributes, inputParameters);
            route += "/";
            var buildRoute = BuildRoute(methodInfo, methodAttributes, inputParameters);
            if (buildRoute.StartsWith("/"))
                route = buildRoute;
            else
                route += buildRoute;
            route = route.Replace("[action]", methodInfo.Name);

            if (!inputParameters.Any(p => p.IsQuery && p is { Used: false, Value: not null }))
                return route;

            var values = inputParameters
                .Where(p => p is { IsQuery: true, Used: false, Value: not null })
                .Select(p =>
                {
                    p.Used = true;
                    return PrepareValueToQuery(p, p.Name);
                }).ToList();
            route += "?" + string.Join("&", values);

            return route;
        }

        private static string BuildRoute(MemberInfo memberInfo, IList<CustomAttributeData> attributes,
            List<InputParameter> inputParameters)
        {
            var routeTemplate = GetRouteTemplate(attributes);
            if (routeTemplate == null) return string.Empty;

            var route = routeTemplate.Replace("[controller]", memberInfo.Name.Replace("Controller", ""));
            foreach (var param in inputParameters.Where(p => p.Value != null))
            {
                var preparedRoute = route
                    .Replace(":guid}", "}")
                    .Replace(":int}", "}")
                    .Replace(":long}", "}")
                    .Replace(":string}", "}");

                var updatedRoute = preparedRoute.Replace($"{{{param.Name}}}", param.Value!.ToString());
                if (updatedRoute != preparedRoute) param.Used = true;
                route = updatedRoute;
            }

            return route;
        }

        private static string? GetRouteTemplate(IList<CustomAttributeData> attributes)
        {
            var routeAttribute = attributes.FirstOrDefault(attr => attr.AttributeType == typeof(RouteAttribute));
            var httpMethodAttribute =
                attributes.FirstOrDefault(attr => attr.AttributeType.BaseType == typeof(HttpMethodAttribute));

            return GetRouteFromAttribute(routeAttribute) ?? GetRouteFromAttribute(httpMethodAttribute);
        }

        private static string? GetRouteFromAttribute(CustomAttributeData? attributeData)
        {
            return attributeData?.ConstructorArguments.FirstOrDefault().Value as string;
        }

        private static string PrepareValueToQuery(InputParameter inputParameter, string name)
        {
            if (inputParameter.Value is DateTimeOffset dto)
            {
                var dtoString = RaitSerializationConfig.DateTimeOffsetToQuery(dto);
                return $"{name}={Uri.EscapeDataString(dtoString)}";
            }
            
            if (inputParameter.Value is DateOnly d)
                return $"{name}={Uri.EscapeDataString(RaitSerializationConfig.DateOnlyToQuery(d))}";
            
            if (inputParameter.Value is DateTime dt)
                return $"{name}={Uri.EscapeDataString(RaitSerializationConfig.DateTimeToQuery(dt))}";

            if (inputParameter.Value is string value)
                return $"{name}={value}";
            if (inputParameter.Value!.GetType().IsValueType)
                return $"{name}={inputParameter.Value}";
            


            return ToQueryString(inputParameter.Value);
        }

        private static string ToQueryString(object request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var properties = request.GetType().GetProperties()
                .Where(prop => prop.CanRead && prop.GetValue(request) != null)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(request));

            var queryParams = properties
                .Where(prop => !(prop.Value is IEnumerable) || prop.Value is string)
                .SelectMany(prop => ValueToString(prop.Value ?? string.Empty, prop))
                .ToList();

            var arrayParams = properties
                .Where(prop => prop.Value is IEnumerable && !(prop.Value is string))
                .SelectMany(prop => ((IEnumerable)prop.Value!).Cast<object>().Select(value => $"{prop.Key}={value}"));

            queryParams.AddRange(arrayParams);

            return string.Join("&", queryParams);
        }

        private static List<string> ValueToString(object value, KeyValuePair<string, object?> prop)
        {
            if (!value.GetType().IsPrimitive && value is not DateTime && value is not string && value is not Guid &&
                value is not decimal)
            {
                return value.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetIndexParameters().Length == 0) // Ensure property is not an indexer
                    .Select(p =>
                    {
                        var stringValue = p.GetValue(value)?.ToString();
                        if (stringValue == null)
                            return null;
                        return $"{Uri.EscapeDataString(prop.Key)}.{p.Name}={Uri.EscapeDataString(stringValue)}";
                    })
                    .Where(n => n != null)
                    .ToList();
            }

            if (value is DateTime dt)
                return new List<string> { $"{Uri.EscapeDataString(prop.Key)}={dt:O}" };
            if (value is Guid guid)
                return new List<string> { $"{Uri.EscapeDataString(prop.Key)}={guid}" };
            if (value is decimal dec)
                return new List<string> { $"{Uri.EscapeDataString(prop.Key)}={dec}" };
            return new List<string> { $"{Uri.EscapeDataString(prop.Key)}={Uri.EscapeDataString(value.ToString()!)}" };
        }
    }
}