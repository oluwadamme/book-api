using FluentValidation;
namespace FirstApi.DTOs;

public class ResetPasswordRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Token { get; set; }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("You must enter an email.")
            .EmailAddress().WithMessage("That is not a valid email format.");
        RuleFor(user => user.Password)
            .NotEmpty().WithMessage("You must enter a password.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain 1 uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain 1 number.")
            .Matches("[!@#$%^&*]").WithMessage("Password must contain 1 special character.");
        RuleFor(user => user.Token)
            .NotEmpty().WithMessage("You must enter a token.");
    }
}