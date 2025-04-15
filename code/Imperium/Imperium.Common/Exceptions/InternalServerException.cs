using System.Net;

namespace Imperium.Common.Exceptions;

public class InternalServerException : ServiceException
{
    public InternalServerException() : base(HttpStatusCode.InternalServerError)
    {
    }

    public InternalServerException(string error) : base(HttpStatusCode.InternalServerError, error)
    {
    }
}
