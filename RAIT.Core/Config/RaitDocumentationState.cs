namespace RAIT.Core;

internal class RaitDocumentationState
{
    internal static XmlDoc.XmlDocRootModel? DocRootModelState { get; set; }
    internal static string? ResultPath { get; set; }
    internal static bool DocGeneration;
}