using System.Text.Json;

namespace blog_api.Exception;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (System.Exception e)
        {
            await HandleExceptionAsync(context, e);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, System.Exception exception)
    {
        var code = exception switch
        {
            BlogApiArgumentException => 400,
            BlogApiSecurityException => 403,
            BlogApiUnauthorizedAccessException => 401,
            _ => 500
        };
        var message = exception switch
        {
            BlogApiArgumentException blogApiArgumentException => JsonSerializer.Serialize(new
                { message = blogApiArgumentException.Message }),
            BlogApiSecurityException blogApiSecurityException => JsonSerializer.Serialize(new
                { message = blogApiSecurityException.Message }),
            BlogApiUnauthorizedAccessException blogApiUnauthorizedAccessException => JsonSerializer.Serialize(new
                { message = blogApiUnauthorizedAccessException.Message }),
            _ => JsonSerializer.Serialize(new { message = "An error occured while processing your request" })
        };
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = code;
        return context.Response.WriteAsync(message);
    }
}