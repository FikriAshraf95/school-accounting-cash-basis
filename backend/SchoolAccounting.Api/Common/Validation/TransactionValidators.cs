using FluentValidation;
using SchoolAccounting.Api.Features.Transactions;

namespace SchoolAccounting.Api.Common.Validation;

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    private static readonly string[] ValidTypes = { "income", "expense" };
    private static readonly string[] ValidTransactableTypes = { "Student", "Payer" };
    private static readonly string[] ValidPaymentMethods = { "cash", "bank_transfer", "check", "online", "other" };

    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Transaction date cannot be in the future");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .Must(type => ValidTypes.Contains(type.ToLower()))
            .WithMessage($"Type must be one of: {string.Join(", ", ValidTypes)}");

        RuleFor(x => x.TransactableType)
            .NotEmpty().WithMessage("Transactable type is required")
            .Must(type => ValidTransactableTypes.Contains(type))
            .WithMessage($"Transactable type must be one of: {string.Join(", ", ValidTransactableTypes)}");

        RuleFor(x => x)
            .Must(x => x.StudentId.HasValue || x.PayerId.HasValue)
            .WithMessage("Either StudentId or PayerId must be provided");

        RuleFor(x => x.CashLedgerId)
            .GreaterThan(0).WithMessage("Cash ledger is required");

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(50).WithMessage("Payment method must not exceed 50 characters")
            .Must(method => string.IsNullOrEmpty(method) || ValidPaymentMethods.Contains(method.ToLower()))
            .WithMessage($"Payment method must be one of: {string.Join(", ", ValidPaymentMethods)}")
            .When(x => !string.IsNullOrEmpty(x.PaymentMethod));

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithMessage("Reference number must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one transaction item is required")
            .Must(items => items.Count > 0).WithMessage("At least one transaction item is required");

        RuleForEach(x => x.Items).SetValidator(new CreateTransactionItemRequestValidator());
    }
}

public class CreateTransactionItemRequestValidator : AbstractValidator<CreateTransactionItemRequest>
{
    public CreateTransactionItemRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be 0 or greater");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
    private static readonly string[] ValidPaymentMethods = { "cash", "bank_transfer", "check", "online", "other" };

    public UpdateTransactionRequestValidator()
    {
        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required");

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(50).WithMessage("Payment method must not exceed 50 characters")
            .Must(method => string.IsNullOrEmpty(method) || ValidPaymentMethods.Contains(method.ToLower()))
            .WithMessage($"Payment method must be one of: {string.Join(", ", ValidPaymentMethods)}")
            .When(x => !string.IsNullOrEmpty(x.PaymentMethod));

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100).WithMessage("Reference number must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one transaction item is required")
            .Must(items => items.Count > 0).WithMessage("At least one transaction item is required");

        RuleForEach(x => x.Items).SetValidator(new CreateTransactionItemRequestValidator());
    }
}
