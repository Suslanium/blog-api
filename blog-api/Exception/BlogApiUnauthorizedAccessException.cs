namespace blog_api.Exception;

public class BlogApiUnauthorizedAccessException : System.Exception
{
    public BlogApiUnauthorizedAccessException(string message)
        : base(message)
    { }

    public BlogApiUnauthorizedAccessException(string message, System.Exception inner)
        : base(message, inner)
    { }
}