using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using RAIT.Core.XmlDoc;

namespace RAIT.Core
{
    internal class RaitDocumentationGenerator
    {
        public class PropertyExample
        {
            public string? Type { get; set; }
            public string? Assembly { get; set; }
            public string? Value { get; set; }
        }

        private static void Generate(RaitDocumentationReport raitDocumentationReport)
        {
            var files = Directory.GetFiles(AppContext.BaseDirectory);
            var dlls = files.Where(n => n.EndsWith(".dll"));
            var xmlDocs = files.Where(n => n.EndsWith(".xml")
                                           && dlls.Contains(n.Replace(".xml", ".dll")));

            foreach (var xmlDocFilePath in xmlDocs)
            {
                var serializer = new XmlSerializer(typeof(XmlDocRootModel));
                using var reader = XmlReader.Create(xmlDocFilePath);
                var doc = (XmlDocRootModel)serializer.Deserialize(reader)!;

                UpdatePropertyExamples(doc, raitDocumentationReport);
                UpdateMethods(doc, raitDocumentationReport);

                SaveXmlDoc(serializer, doc, xmlDocFilePath);
                RaitDocumentationState.DocRootModelState = doc;
            }
        }

        private static void UpdatePropertyExamples(XmlDocRootModel doc, RaitDocumentationReport report)
        {
            if (doc.Assembly?.Name == null)
                return;
            if (doc.Members?.Member == null)
                return;

            if (!report.PropertyExamples.TryGetValue(doc.Assembly.Name, out var propertyExamples))
                return;

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

        private static void UpdateMethods(XmlDocRootModel doc, RaitDocumentationReport report)
        {
            if (doc.Assembly?.Name == null)
                return;
            if (doc.Members?.Member == null)
                return;

            if (!report.Methods.TryGetValue(doc.Assembly.Name, out var methods))
                return;

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

        private static void SaveXmlDoc(XmlSerializer serializer, XmlDocRootModel doc, string xmlDocFilePath)
        {
            try
            {
                var resultPath = Path.Combine(RaitDocumentationState.ResultPath ?? AppContext.BaseDirectory,
                    Path.GetFileName(xmlDocFilePath.Replace(".xml", "_rait.xml")));
                using var writer = new StreamWriter(resultPath);
                serializer.Serialize(writer, doc);
            }
            catch (Exception)
            {
                // Handle exception
            }
        }

        private static void ChangeCommentMethod(Member propertyMember, KeyValuePair<string, PropertyExample> valuePair)
        {
            var text = CreateTestedText();
            propertyMember.Summary =
                string.IsNullOrEmpty(propertyMember.Summary) ? text : text + propertyMember.Summary;
        }

        private static string CreateTestedText() => "[Tested]";

        private static Member CreatePropertyMemberMethod(KeyValuePair<string, PropertyExample> valuePair) =>
            new Member
            {
                Name = valuePair.Key,
                Summary = CreateTestedText()
            };

        private static Member CreatePropertyMember(KeyValuePair<string, PropertyExample> propertyMember) =>
            new Member
            {
                Name = propertyMember.Key,
                Example = propertyMember.Value.Value
            };

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

        const string RaitDocReport = "RAIT.json";

        public static void Params<TController>(List<InputParameter> prepareInputParameters)
        {
            if (!RaitDocumentationState.DocumentationGenerationEnabled) return;

            CreatePath<TController>();

            var raitDocumentationReport =
                RecoveryDoc(Path.Combine(RaitDocumentationState.ResultPath ?? "", RaitDocReport));

            UpdateRaitDoc(prepareInputParameters, raitDocumentationReport);

            Generate(raitDocumentationReport);
            Save(raitDocumentationReport, Path.Combine(RaitDocumentationState.ResultPath ?? "", RaitDocReport));
        }

        private static void CreatePath<TController>()
        {
            var assembly = typeof(TController).Assembly;
            var directoryName = Path.GetDirectoryName(assembly.Location)!;
            var resultPath = Path.Combine(directoryName, @"..\..\..\..\" + assembly.GetName().Name + @"\RAIT\");
            Directory.CreateDirectory(resultPath);
            RaitDocumentationState.ResultPath ??= resultPath;
        }

        private static void Save(RaitDocumentationReport raitDocumentationReport, string raitDocReport)
        {
            var serialize = JsonSerializer.Serialize(raitDocumentationReport, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            try
            {
                File.WriteAllText(raitDocReport, serialize);
            }
            catch (Exception)
            {
                // Handle exception
            }
        }

        private static void UpdateRaitDoc(List<InputParameter> prepareInputParameters,
            RaitDocumentationReport raitDocumentationReport)
        {
            var inputParameters = prepareInputParameters.Where(n => n.Type != null).ToList();
            foreach (var parameter in inputParameters.ToList())
            {
                if (parameter.Type == typeof(string)) continue;

                var assembly = parameter.Type!.Assembly.GetName().Name!;
                foreach (var propertyInfo in parameter.Type.GetProperties()
                             .Where(p => p.GetIndexParameters().Length == 0))
                {
                    if (propertyInfo.GetCustomAttribute<RaitDocIgnoreAttribute>() != null) continue;
                    if (parameter.Value == null)
                        continue;
                    var value = propertyInfo.GetValue(parameter.Value);
                    if (value == null) continue;

                    var example = new PropertyExample
                    {
                        Assembly = assembly,
                        Type = $"P:{parameter.Type}.{propertyInfo.Name}",
                        Value = value.ToStringParam()
                    };

                    if (example.Value == null && value is IEnumerable enumerable)
                    {
                        var firstOrDefault = enumerable.Cast<object>().FirstOrDefault();
                        if (firstOrDefault != null)
                        {
                            inputParameters.Add(new InputParameter
                            {
                                Value = firstOrDefault,
                                Type = firstOrDefault.GetType()
                            });
                        }
                    }
                    else if (example.Value == null)
                    {
                        inputParameters.Add(new InputParameter
                        {
                            Value = value,
                            Type = value.GetType()
                        });
                    }

                    raitDocumentationReport.PropertyExamples.TryAdd(assembly,
                        new Dictionary<string, PropertyExample>());
                    raitDocumentationReport.PropertyExamples[assembly][example.Type] = example;
                }
            }
        }

        private static void UpdateRaitDocMethod(Type type, string memberKey,
            RaitDocumentationReport raitDocumentationReport)
        {
            var assembly = type.Assembly.GetName().Name!;
            var example = new PropertyExample
            {
                Assembly = assembly,
                Type = memberKey
            };

            raitDocumentationReport.Methods.TryAdd(assembly, new Dictionary<string, PropertyExample>());
            raitDocumentationReport.Methods[assembly][memberKey] = example;
        }

        private static RaitDocumentationReport RecoveryDoc(string raitDocReport)
        {
            if (!File.Exists(raitDocReport))
                return new RaitDocumentationReport();

            var readAllText = File.ReadAllText(raitDocReport);
            return JsonSerializer.Deserialize<RaitDocumentationReport>(readAllText) ?? new RaitDocumentationReport();
        }

        public static void Method<TController>(string methodInfoName, List<InputParameter> prepareInputParameters)
            where TController : ControllerBase
        {
            var type = typeof(TController);
            var memberKey = $"M:{type.FullName}.{methodInfoName}";

            if (prepareInputParameters.Any())
            {
                memberKey += $"({string.Join(',', prepareInputParameters.Select(n => n.Type?.ToString()))})";
            }

            CreatePath<TController>();

            var raitDocumentationReport =
                RecoveryDoc(Path.Combine(RaitDocumentationState.ResultPath ?? "", RaitDocReport));

            UpdateRaitDocMethod(type, memberKey, raitDocumentationReport);

            Generate(raitDocumentationReport);
            Save(raitDocumentationReport, Path.Combine(RaitDocumentationState.ResultPath ?? "", RaitDocReport));
        }
    }
}