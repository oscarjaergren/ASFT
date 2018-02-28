using System;

namespace DataTypes
{
    public abstract class ApiException : Exception
    {
        protected ApiException()
        {
        }

        protected ApiException(string message) : base(message)
        {
        }

        protected ApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class UserNotFoundException : Exception
    {
    }

    public class InvalidCredentialsException : ApiException
    {
    }

    public class NotUniqueException : ApiException
    {
    }

    public class LocationNotFoundException : ApiException
    {
    }

    public class IssueNotFoundException : ApiException
    {
    }

    public class IssueImageNotFoundException : ApiException
    {
    }

    public class InvalidIssueSeverityException : ApiException
    {
    }

    public class InvalidIssueStatusException : ApiException
    {
    }

    public class InvalidFileTypeException : ApiException
    {
    }
}