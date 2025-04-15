using System.Net;

namespace Imperium.Common.Exceptions;

public class UnauthorizedException : ServiceException
{
    public UnauthorizedException() : base(HttpStatusCode.Unauthorized)
    {
    }
}
