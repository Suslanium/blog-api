using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace blog_api.Exception;

public class ExceptionHandlingFilter : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        ProblemDetails problemDetails;
        
        if (context.Exception is BlogApiException blogApiException)
        {
            problemDetails = new ProblemDetails
            {
                Title = blogApiException.Message,
                Status = blogApiException.StatusCode
            };
        }
        else
        {
            problemDetails = new ProblemDetails
            {
                Title = "An error occured while processing your request",
                Status = 500
            };
        }

        context.Result = new ObjectResult(problemDetails);

        context.ExceptionHandled = true;
    }
}