using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FurnitureShop.API.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var failures = new List<ValidationFailure>();

        foreach (var (_, argument) in context.ActionArguments)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator is null) continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
                failures.AddRange(result.Errors);
        }

        if (failures.Count > 0)
            throw new ValidationException(failures);

        await next();
    }
}