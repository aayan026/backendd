namespace FurnitureShop.Application.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Giriş tələb olunur") 
        : base(message) { }
}
