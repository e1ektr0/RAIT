namespace RAIT.Core;

internal static class RaitDocumentationConfig
{
    public static void Enable()
    {
        RaitDocumentationState.DocumentationGenerationEnabled = true;
    }
}

internal static class RaitDocumentationState
{
    internal static XmlDoc.XmlDocRootModel? DocRootModelState { get; set; }
    internal static string? ResultPath { get; set; }
    internal static bool DocumentationGenerationEnabled;
}