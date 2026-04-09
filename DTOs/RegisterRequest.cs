using FluentValidation;

namespace FirstApi.DTOs;
// 1. The DTO is now a "pure" data bag. No attributes!
public class RegisterRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

// 2. The dedicated Validator class
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(user => user.FirstName)
            .NotEmpty().WithMessage("You must enter a first name.")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters.");
        RuleFor(user => user.LastName)
            .NotEmpty().WithMessage("You must enter a last name.")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters.");
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("You must enter an email.")
            .EmailAddress().WithMessage("That is not a valid email format.");

        RuleFor(user => user.Password)
            .NotEmpty()
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain 1 uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain 1 number.")
            .Matches("[!@#$%^&*]").WithMessage("Password must contain 1 special character.");
    }
}
