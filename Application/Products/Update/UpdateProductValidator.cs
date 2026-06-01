using Domain.Products;
using FluentValidation;

namespace Application.Products.Update;

public class UpdateProductValidator : AbstractValidator<Product>
{
    public UpdateProductValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200);

        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(p => p.InternalCost)
            .GreaterThanOrEqualTo(0).WithMessage("Internal cost cannot be negative.");

        RuleFor(p => p.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

        RuleFor(p => p.Category)
            .NotEmpty().WithMessage("Category is required.");
    }
}
