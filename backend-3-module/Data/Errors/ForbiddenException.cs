using System.Net;

namespace backend_3_module.Data.Errors;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(HttpStatusCode.Forbidden, message)
    {
    }
}