namespace FurnitureShop.Application.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Bu əməliyyata icazəniz yoxdur") 
        : base(message) { }
}
