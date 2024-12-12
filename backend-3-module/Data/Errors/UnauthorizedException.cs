using System.Net;

namespace backend_3_module.Data.Errors;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(HttpStatusCode.Unauthorized, message)
    {
    }
}