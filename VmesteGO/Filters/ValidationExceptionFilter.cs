using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace VmesteGO.Filters;

public class ValidationExceptionFilter : ExceptionFilterAttribute
{
    public override Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not ValidationException validationException)
        {
            return Task.CompletedTask;
        }

        var errors = validationException.Errors.Select(error => new
        {
            PropertyName = error.PropertyName.IsNullOrEmpty()
                ? "The validated value is not a property"
                : error.PropertyName,
            error.ErrorMessage
        }).ToList();

        var result = new BadRequestObjectResult(errors);
        context.Result = result;
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}