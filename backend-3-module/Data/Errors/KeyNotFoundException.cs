using System.Net;

namespace backend_3_module.Data.Errors;

public class KeyNotFoundException : AppException
{
    public KeyNotFoundException(string message) : base(HttpStatusCode.NotFound, message)
    {
    }
}