using FluentValidation;

namespace ModularMonolith.Modules.Customers.Application.Commands.CreateCustomer;

internal sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleForEach(x => x.Addresses)
            .ChildRules(address =>
            {
                address.RuleFor(a => a.Street).NotEmpty().WithMessage("Street is required");
                address.RuleFor(a => a.City).NotEmpty().WithMessage("City is required");
                address.RuleFor(a => a.Country).NotEmpty().WithMessage("Country is required");
                address.RuleFor(a => a.Type).IsInEnum().WithMessage("Invalid address type");
            })
            .When(x => x.Addresses != null);
    }
}
