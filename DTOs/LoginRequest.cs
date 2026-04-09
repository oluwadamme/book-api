using FluentValidation;
namespace FirstApi.DTOs;

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("You must enter an email.")
            .EmailAddress().WithMessage("That is not a valid email format.");
        RuleFor(user => user.Password)
            .NotEmpty().WithMessage("You must enter a password.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}