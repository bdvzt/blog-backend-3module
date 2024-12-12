using System.Net;

namespace backend_3_module.Data.Errors;

public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(HttpStatusCode.BadRequest, message)
    {
    }
}