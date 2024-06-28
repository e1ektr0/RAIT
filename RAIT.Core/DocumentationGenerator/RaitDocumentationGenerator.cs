using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using RAIT.Core.DocumentationGenerator.XmlDoc;

namespace RAIT.Core.DocumentationGenerator;

internal class RaitDocumentationGenerator
{
    public class PropertyExample
    {
        public string Type { get; set; }
        public string Assembly { get; set; }
        public string Value { get; set; }
    }

    private static void Generate(RaitDocumentationReport raitDocumentationReport)
    {
        //todo: parameters for filter
        var files = Directory.GetFiles(AppContext.BaseDirectory);
        var dlls = files.Where(n => n.EndsWith(".dll"));
        var xmlDocs = files.Where(n => n.EndsWith(".xml") && dlls.Contains(n.Replace(".xml", ".dll")));


        foreach (var xmlDocFilePath in xmlDocs)
        {
            var serializer = new XmlSerializer(typeof(XmlDoc.XmlDoc));

            using var reader = XmlReader.Create(xmlDocFilePath);
            var doc = (XmlDoc.XmlDoc)serializer.Deserialize(reader)!;

            if (raitDocumentationReport.PropertyExamples
                .TryGetValue(doc.Assembly.Name, out var propertyExamples))
            {
                foreach (var valuePair in propertyExamples)
                {
                    var propertyMember = doc.Members.Member.FirstOrDefault(n => n.Name == valuePair.Value.Type);
                    if (propertyMember != null)
                    {
                        ChangeComment(propertyMember, valuePair);
                    }
                    else
                    {
                        doc.Members.Member.Add(CreatePropertyMember(valuePair));
                    }
                }
            }


            if (raitDocumentationReport.Methods
                .TryGetValue(doc.Assembly.Name, out var methods))
            {
                foreach (var valuePair in methods)
                {
                    var propertyMember = doc.Members.Member.FirstOrDefault(n => n.Name == valuePair.Value.Type);
                    if (propertyMember != null)
                    {
                        ChangeCommentMethod(propertyMember, valuePair);
                    }
                    else
                    {
                        doc.Members.Member.Add(CreatePropertyMemberMethod(valuePair));
                    }
                }
            }


            using var writer =
                new StreamWriter(Path.Combine(RaitConfig.ResultPath ?? AppContext.BaseDirectory,
                    Path.GetFileName(xmlDocFilePath.Replace(".xml", "_rait.xml"))));
            serializer.Serialize(writer, doc);
            RaitConfig.DocState = doc;
        }
    }

    private static void ChangeCommentMethod(Member propertyMember, KeyValuePair<string, PropertyExample> valuePair)
    {
        var text = CreateTestedText();
        if (string.IsNullOrEmpty(propertyMember.Summary))
        {
            propertyMember.Summary = text;
            return;
        }

        propertyMember.Summary = text + propertyMember.Summary;
    }

    private static string CreateTestedText()
    {
        return "[Tested]";
    }

    private static Member CreatePropertyMemberMethod(KeyValuePair<string, PropertyExample> valuePair)
    {
        return new Member
        {
            Name = valuePair.Key,
            Summary = CreateTestedText() //fix for classes
        };
    }

    private static Member CreatePropertyMember(KeyValuePair<string, PropertyExample> propertyMember)
    {
        return new Member
        {
            Name = propertyMember.Key,
            Example = propertyMember.Value.Value
        };
    }

    private static void ChangeComment(Member propertyMemberText, KeyValuePair<string, PropertyExample> valuePair)
    {
        if (string.IsNullOrEmpty(propertyMemberText.Example))
            propertyMemberText.Example = valuePair.Value.Value;
    }

    public class RaitDocumentationReport
    {
        public Dictionary<string, Dictionary<string, PropertyExample>> PropertyExamples { get; set; } = new();
        public Dictionary<string, Dictionary<string, PropertyExample>> Methods { get; set; } = new();
    }

    const string RaitDocReport = "rait_doc_report";

    public static void Params<TController>(List<InputParameter> prepareInputParameters)
    {
        if (!RaitConfig.DocGeneration)
            return;

        var raitDocumentationReport = RecoveryDoc(RaitDocReport);

        var assembly = typeof(TController).Assembly;
        var directoryName = Path.GetDirectoryName(assembly.Location)!;
        var resultPath = Path.Combine(directoryName, @"..\..\..\..\" + assembly.GetName().Name + @"\RAIT\");
        Directory.CreateDirectory(resultPath);
        RaitConfig.ResultPath = resultPath;
        
        UpdateRaitDoc(prepareInputParameters, raitDocumentationReport);

        //todo: on dispose server
        Generate(raitDocumentationReport);
        Save(raitDocumentationReport, RaitDocReport);
    }

    private static void Save(RaitDocumentationReport raitDocumentationReport, string raitDocReport)
    {
        var serialize = JsonSerializer.Serialize(raitDocumentationReport);
        File.WriteAllText(raitDocReport, serialize);
    }

    private static void UpdateRaitDoc(List<InputParameter> prepareInputParameters,
        RaitDocumentationReport raitDocumentationReport)
    {
        var inputParameters = prepareInputParameters.Where(n => n.Type != null).ToList();
        for (var index = 0; index < inputParameters.Count; index++)
        {
            var parameter = inputParameters[index];
            if (parameter.Type == typeof(string))
                continue;
            var assembly = parameter.Type!.Assembly.GetName().Name!;

            foreach (var propertyInfo in parameter.Type.GetProperties().Where(p => p.GetIndexParameters().Length == 0))
            {
                if (propertyInfo.GetCustomAttribute<RaitDocIgnoreAttribute>() != null)
                    continue;
                var value = propertyInfo.GetValue(parameter.Value);
                if (value == null)
                    continue;
                var example = new PropertyExample
                {
                    Assembly = assembly
                };
                var key = $"P:{parameter.Type}.{propertyInfo.Name}";
                example.Type = key;
                var stringParam = value.ToStringParam();
                if (stringParam != null)
                {
                    example.Value = stringParam!;
                }
                else
                {
                    if (value is IEnumerable)
                    {
                        var enumerable = (IEnumerable)value;
                        var firstOrDefault = enumerable.Cast<object>().ToList().FirstOrDefault();
                        if (firstOrDefault != null)
                        {
                            inputParameters.Add(new InputParameter
                            {
                                Value = firstOrDefault,
                                Type = firstOrDefault.GetType()
                            });
                        }
                    }
                    else
                    {
                        inputParameters.Add(new InputParameter
                        {
                            Value = value,
                            Type = value.GetType()
                        });
                    }
                }

                raitDocumentationReport.PropertyExamples.TryAdd(assembly,
                    new Dictionary<string, PropertyExample>());

                raitDocumentationReport.PropertyExamples[assembly].TryAdd(key, example);
                raitDocumentationReport.PropertyExamples[assembly][key] = example;
            }
        }
    }

    private static void UpdateRaitDocMethod(Type type, string memberKey,
        RaitDocumentationReport raitDocumentationReport)
    {
        {
            var assembly = type.Assembly.GetName().Name!;

            {
                var example = new PropertyExample
                {
                    Assembly = assembly,
                    Type = memberKey
                };

                raitDocumentationReport.Methods.TryAdd(assembly,
                    new Dictionary<string, PropertyExample>());

                raitDocumentationReport.Methods[assembly].TryAdd(memberKey, example);
                raitDocumentationReport.Methods[assembly][memberKey] = example;
            }
        }
    }


    private static RaitDocumentationReport RecoveryDoc(string raitDocReport)
    {
        //todo:cache in memory
        RaitDocumentationReport? raitDocumentationReport;
        if (File.Exists(raitDocReport))
        {
            var readAllText = File.ReadAllText(raitDocReport);
            raitDocumentationReport = JsonSerializer.Deserialize<RaitDocumentationReport>(readAllText);
        }
        else
        {
            raitDocumentationReport = new RaitDocumentationReport();
        }

        return raitDocumentationReport!;
    }

    public static void Method<TController>(string methodInfoName, List<InputParameter> prepareInputParameters)
        where TController : ControllerBase
    {
        var type = typeof(TController);

        var memberKey = $"M:{type.FullName}.{methodInfoName}";
        if (prepareInputParameters.Any())
        {
            memberKey += $"({string.Join(',', prepareInputParameters.Select(n => n.Type.ToString()))})";
        }

        var raitDocumentationReport = RecoveryDoc(RaitDocReport);

        UpdateRaitDocMethod(type, memberKey, raitDocumentationReport);

        //todo: on dispose server
        Generate(raitDocumentationReport);
        Save(raitDocumentationReport, RaitDocReport);
    }
}