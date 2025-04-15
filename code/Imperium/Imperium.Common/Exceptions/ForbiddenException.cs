using System.Net;

namespace Imperium.Common.Exceptions;

public class ForbiddenException : ServiceException
{
    public ForbiddenException() : base(HttpStatusCode.Forbidden)
    {
    }
}
