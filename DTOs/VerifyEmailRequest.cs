using FluentValidation;
namespace FirstApi.DTOs;

public class VerifyEmailRequest
{
    public string Email { get; set; }
    public string Token { get; set; }
}

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("You must enter an email.")
            .EmailAddress().WithMessage("That is not a valid email format.");
        RuleFor(user => user.Token)
            .NotEmpty().WithMessage("You must enter a token.");
    }
}