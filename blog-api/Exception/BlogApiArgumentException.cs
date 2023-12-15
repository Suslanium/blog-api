namespace blog_api.Exception;

public class BlogApiArgumentException : System.Exception
{
    public BlogApiArgumentException(string message)
        : base(message)
    { }

    public BlogApiArgumentException(string message, System.Exception inner)
        : base(message, inner)
    { }
}