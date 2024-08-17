using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RAIT.Core;

public static class RaitSwaggerConfigExtension
{
    public static void IncludeRaitXml(this SwaggerGenOptions swaggerGenOptions)
    {
        var executingAssembly = Assembly.GetEntryAssembly();

        var assemblies = executingAssembly!.GetReferencedAssemblies().ToList();
        assemblies.Add(executingAssembly.GetName());
        var files = assemblies
            .Where(assembly => assembly.Name != null)
            .Select(assembly =>
                Path.Combine(AppContext.BaseDirectory, Path.Combine("RAIT", $"{assembly.Name}_rait.xml"))).ToList();
        RaitLogger.Log("Files", files);
        var referencedProjectsXmlDocPaths = files
            .Where(File.Exists).ToList();
        RaitLogger.Log("referencedProjectsXmlDocPaths", referencedProjectsXmlDocPaths);

        Directory.GetFiles("real Files base:", AppContext.BaseDirectory);
        Directory.GetFiles("rait real Files:", Path.Combine(AppContext.BaseDirectory, "RAIT"));
        foreach (var xmlDocPath in referencedProjectsXmlDocPaths)
        {
            swaggerGenOptions.IncludeXmlComments(xmlDocPath);
        }
    }
}