using System.Net;
using backend_3_module.Data.DTO;
using backend_3_module.Data.Errors;

namespace backend_3_module.Middlewares;

public class ExeptionHandlingMiddleWare
{
    private readonly RequestDelegate _next;

    public ExeptionHandlingMiddleWare(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (AppException ex)
        {
            await HandleExceptionAsync(httpContext, ex.Message, ex.StatusCode);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex.Message, HttpStatusCode.InternalServerError);
        }
    }

    public async Task HandleExceptionAsync(
        HttpContext httpContext,
        string exMessage,
        HttpStatusCode httpStatusCode)
    {
        HttpResponse response = httpContext.Response;

        response.ContentType = "application/json";
        response.StatusCode = (int)httpStatusCode;

        var errorDto = new ErrorDTO
        {
            Message = exMessage,
            StatusCode = (int)httpStatusCode
        };

        await response.WriteAsJsonAsync(errorDto);
    }
}