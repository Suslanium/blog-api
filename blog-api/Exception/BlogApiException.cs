namespace blog_api.Exception;

public class BlogApiException : System.Exception
{
    public readonly int StatusCode;

    public BlogApiException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public BlogApiException(int statusCode, string message, System.Exception inner)
        : base(message, inner)
    {
        StatusCode = statusCode;
    }
}