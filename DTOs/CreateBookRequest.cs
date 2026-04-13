using FluentValidation;
namespace FirstApi.DTOs;

public class CreateBookRequest
{
    public string Title { get; set; }
    public string Author { get; set; }
    public int YearPublished { get; set; }
}

public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(book => book.Title)
            .NotEmpty().WithMessage("You must enter a title.")
            .MinimumLength(2).WithMessage("Title must be at least 2 characters.");
        RuleFor(book => book.Author)
            .NotEmpty().WithMessage("You must enter an author.")
            .MinimumLength(2).WithMessage("Author must be at least 2 characters.");
        RuleFor(book => book.YearPublished)
            .NotEmpty().WithMessage("You must enter a year.")
            .LessThanOrEqualTo(DateTime.UtcNow.Year)
            .WithMessage("A book cannot be published in the future!");
    }
}
