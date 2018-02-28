using System;
using System.Net;

namespace IssueManagerApiClient
{
    // These exceptions can occur with most requests

    #region Network related

    public class NotLoggedInException : Exception
    {
    } // Thrown by client if access token is missing

    public class UnauthorizedException : Exception
    {
    } // Only requests that require authentication throws this

    public class ServerNotFoundException : Exception
    {
    }

    public class BadResponseFormatException : Exception
    {
    }

    public class UnexpectedHttpErrorException : Exception
    {
        public UnexpectedHttpErrorException(HttpStatusCode httpErrorCode)
        {
            HttpStatusCode = httpErrorCode;
        }

        public HttpStatusCode HttpStatusCode { get; }
    }

    #endregion Network related

    // These exceptions are action specific. Check the action's documentation to see which ones you need to handle.
//#region Application related
//	public class NotUniqueException : Exception { }
//	public class LocationNotFoundException : Exception { }
//	public class IssueNotFoundException : Exception { }
//	public class IssueImageNotFoundException : Exception { }
//#endregion Application related
}