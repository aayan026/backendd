namespace FurnitureShop.Application.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string key = "AccessDenied")
        : base(key) { }
}