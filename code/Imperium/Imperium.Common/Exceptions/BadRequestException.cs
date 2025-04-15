using System.Net;

namespace Imperium.Common.Exceptions;

public class BadRequestException : ServiceException
{
    public BadRequestException(IList<ServiceError> errors) : base(HttpStatusCode.BadRequest, errors)
    {
    }

    public BadRequestException() : base(HttpStatusCode.BadRequest)
    {
    }

    public BadRequestException(string error) : base(HttpStatusCode.BadRequest, error)
    {
    }
}
