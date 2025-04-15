using System.Net;

namespace Imperium.Common.Exceptions;

public class ServiceException(HttpStatusCode statusCode, IList<ServiceError> errors) : Exception
{
    public ServiceException(HttpStatusCode statusCode)
        : this(statusCode, [])
    {

    }

    public ServiceException(HttpStatusCode statusCode, string error)
        : this(statusCode, new List<ServiceError>([new ServiceError(error)]))
    {

    }

    public ServiceException(HttpStatusCode statusCode, string error, string property)
        : this(statusCode, new List<ServiceError>([new ServiceError(error, property)]))
    {

    }

    public IList<ServiceError> Errors { get; set; } = errors;

    public HttpStatusCode StatusCode { get; set; } = statusCode;
}
