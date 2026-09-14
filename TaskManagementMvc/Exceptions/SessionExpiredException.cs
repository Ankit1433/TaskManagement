namespace TaskManagementMvc.Exceptions;

public class SessionExpiredException : Exception
{
    public SessionExpiredException()
        : base("Your session has expired.")
    {
    }
}