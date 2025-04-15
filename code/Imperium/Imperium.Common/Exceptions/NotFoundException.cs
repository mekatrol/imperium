using System.Net;

namespace Imperium.Common.Exceptions;

public class NotFoundException : ServiceException
{
    public NotFoundException() : base(HttpStatusCode.NotFound)
    {
    }

    public NotFoundException(string error) : base(HttpStatusCode.NotFound, error)
    {
    }

    public NotFoundException(string error, string property) : base(HttpStatusCode.NotFound, error, property)
    {
    }
}
