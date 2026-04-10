using FluentValidation;
using SchoolAccounting.Api.Features.Ledgers;

namespace SchoolAccounting.Api.Common.Validation;

public class CreateLedgerRequestValidator : AbstractValidator<CreateLedgerRequest>
{
    private static readonly string[] ValidTypes = { "asset", "liability", "equity", "revenue", "expense" };

    public CreateLedgerRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(20).WithMessage("Code must not exceed 20 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .Must(type => ValidTypes.Contains(type.ToLower()))
            .WithMessage($"Type must be one of: {string.Join(", ", ValidTypes)}");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Category));
    }
}

public class UpdateLedgerRequestValidator : AbstractValidator<UpdateLedgerRequest>
{
    public UpdateLedgerRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(20).WithMessage("Code must not exceed 20 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Category));
    }
}

public class YearEndCloseRequestValidator : AbstractValidator<YearEndCloseRequest>
{
    public YearEndCloseRequestValidator()
    {
        RuleFor(x => x.Year)
            .GreaterThan(2000).WithMessage("Year must be greater than 2000")
            .LessThanOrEqualTo(2100).WithMessage("Year must not exceed 2100");
    }
}

public class YearBeginningOpenRequestValidator : AbstractValidator<YearBeginningOpenRequest>
{
    public YearBeginningOpenRequestValidator()
    {
        RuleFor(x => x.Year)
            .GreaterThan(2000).WithMessage("Year must be greater than 2000")
            .LessThanOrEqualTo(2100).WithMessage("Year must not exceed 2100");
    }
}
