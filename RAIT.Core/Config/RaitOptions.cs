namespace RAIT.Core;

public enum RaitTypedResultFailureBehavior
{
    ReturnTypedResult = 0,
    ThrowRaitHttpException = 1
}

public sealed class RaitOptions
{
    public RaitTypedResultFailureBehavior TypedResultFailureBehavior { get; set; } =
        RaitGlobalConfiguration.TypedResultFailureBehavior;
}

public static class RaitGlobalConfiguration
{
    public static RaitTypedResultFailureBehavior TypedResultFailureBehavior { get; set; } =
        RaitTypedResultFailureBehavior.ReturnTypedResult;
}
