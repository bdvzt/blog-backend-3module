using System.Net;

namespace backend_3_module.Data.Errors;

public class InternalServerErrorException : AppException
{
    public InternalServerErrorException(string message) : base(HttpStatusCode.InternalServerError, message)
    {
    }
}