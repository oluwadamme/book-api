using FluentValidation;
namespace FirstApi.DTOs;

public class RefreshTokenRequest
{
    public string? RefreshToken { get; set; }
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(user => user.RefreshToken)
            .NotEmpty().WithMessage("You must enter a refresh token.");
    }
}