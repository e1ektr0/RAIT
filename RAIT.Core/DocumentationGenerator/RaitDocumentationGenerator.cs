using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using RAIT.Core.DocumentationGenerator.XmlDoc;

namespace RAIT.Core.DocumentationGenerator;

internal class RaitDocumentationGenerator
{
    public class PropertyExample
    {
        public string Type { get; set; }
        public string SerializedValue { get; set; }
        public string Assembly { get; set; }
        public string Value { get; set; }
    }

    public static void Generate(RaitDocumentationReport raitDocumentationReport)
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

            if (!raitDocumentationReport.PropertyExamples
                    .TryGetValue(doc.Assembly.Name, out var propertyExamples))
                continue;

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

            using var writer =
                new StreamWriter(Path.Combine(AppContext.BaseDirectory,
                    xmlDocFilePath.Replace(".xml", "_rait.xml")));
            serializer.Serialize(writer, doc);
            RaitConfig.DocState = doc;
        }
    }

    private static Member CreatePropertyMember(KeyValuePair<string, PropertyExample> propertyMember)
    {
        return new Member
        {
            Name = propertyMember.Key,
            Example = propertyMember.Value.Value //fix for classes
        };
    }

    private static void ChangeComment(Member propertyMemberText, KeyValuePair<string, PropertyExample> valuePair)
    {
        propertyMemberText.Example = valuePair.Value.Value; //fix for classes
    }

    public class RaitDocumentationReport
    {
        public Dictionary<string, Dictionary<string, PropertyExample>> PropertyExamples { get; set; } = new();
    }

    public static void Params(List<InputParameter> prepareInputParameters)
    {
        if (!RaitConfig.DocGeneration)
            return;

        var raitDocReport = "rait_doc_report";
        var raitDocumentationReport = RecoveryDoc(raitDocReport);

        UpdateRaitDoc(prepareInputParameters, raitDocumentationReport);

        Generate(raitDocumentationReport);

        //todo: on dispose server
        Save(raitDocumentationReport, raitDocReport);
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

            foreach (var propertyInfo in parameter.Type.GetProperties())
            {
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
                    inputParameters.Add(new InputParameter
                    {
                        Value = value,
                        Type = value.GetType()
                    });
                }

                if (raitDocumentationReport.PropertyExamples.TryAdd(assembly,
                        new Dictionary<string, PropertyExample>()))
                    continue;

                raitDocumentationReport.PropertyExamples[assembly].TryAdd(key, example);
                raitDocumentationReport.PropertyExamples[assembly][key] = example;
            }
        }
    }

    private static RaitDocumentationReport RecoveryDoc(string raitDocReport)
    {
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
}