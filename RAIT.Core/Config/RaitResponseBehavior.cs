using RAIT.Core.Http;

namespace RAIT.Core;

internal static class RaitResponseBehavior
{
    internal static bool ShouldThrowForNonSuccess(Type responseType)
    {
        if (RaitGlobalConfiguration.TypedResultFailureBehavior == RaitTypedResultFailureBehavior.ThrowRaitHttpException)
            return true;

        return !TypedResultsHandler.IsTypedHttpResultType(responseType) &&
               !TypedResultsHandler.IsResultsUnionType(responseType);
    }
}
