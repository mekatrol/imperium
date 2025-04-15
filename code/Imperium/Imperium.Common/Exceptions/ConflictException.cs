using System.Net;

namespace Imperium.Common.Exceptions;

public class ConflictException : ServiceException
{
    public ConflictException() : base(HttpStatusCode.Conflict)
    {
    }

    public ConflictException(string error) : base(HttpStatusCode.Conflict, error)
    {
    }
}
