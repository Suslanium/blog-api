namespace blog_api.Exception;

public class BlogApiSecurityException : System.Exception
{
    public BlogApiSecurityException(string message)
        : base(message)
    { }

    public BlogApiSecurityException(string message, System.Exception inner)
        : base(message, inner)
    { }
}