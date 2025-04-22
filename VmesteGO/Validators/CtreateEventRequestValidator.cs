using FluentValidation;
using VmesteGO.Dto.Requests;

namespace VmesteGO.Validators;

public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AgeRestriction)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Dates)
            .GreaterThan(DateTime.UtcNow).WithMessage("Event date must be in the future");

        RuleFor(x => x.Location)
            .NotEmpty();
    }
}