namespace FurnitureShop.Application.Exceptions;

public class ValidationException : Exception
{
    public Dictionary<string, List<string>> Errors { get; }

    public ValidationException(Dictionary<string, List<string>> errors)
        : base("ValidationError")
    {
        Errors = errors;
    }
}