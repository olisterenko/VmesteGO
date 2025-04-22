using FluentValidation;
using VmesteGO.Domain.Entities;
using VmesteGO.Dto.Requests;
using VmesteGO.Specifications.UserSpecs;

namespace VmesteGO.Validators;

public class UserLoginRequestValidator : AbstractValidator<UserLoginRequest>
{
    public UserLoginRequestValidator(IRepository<User> userRepository)
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(30).WithMessage("Max 30 characters")
            .MustAsync(async (username, cancellationToken) =>
            {
                var userSpec = new UserByUsernameSpec(username);
                var existingUser = await userRepository.FirstOrDefaultAsync(userSpec, cancellationToken);
                return existingUser == null;
            }).WithMessage("Username already exists");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Min 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one upper case letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lower case")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit");
    }
}