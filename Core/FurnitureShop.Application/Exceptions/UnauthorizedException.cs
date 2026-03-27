namespace FurnitureShop.Application.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string key = "LoginRequired")
        : base(key) { }
}