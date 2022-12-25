using System.Net;

namespace RAIT.Core;

public class RaitHttpException : Exception
{
    // ReSharper disable once MemberCanBePrivate.Global
    public HttpStatusCode StatusCode { get; }

    public RaitHttpException(string? message, HttpStatusCode httpStatusCode) : base(message)
    {
        StatusCode = httpStatusCode;
    }
}