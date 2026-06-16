using Application.Data;
using Domain.Vendors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Vendors.Create;

internal sealed class CreateVendorValidator : AbstractValidator<CreateVendorCommand>
{
    public CreateVendorValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(v => v.TaxId).NotEmpty().MaximumLength(32);
    }
}

internal sealed class CreateVendorCommandHandler : IRequestHandler<CreateVendorCommand, Guid>
{
    private readonly CreateVendorValidator _validator;
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateVendorCommandHandler> _logger;

    public CreateVendorCommandHandler(
        IVendorRepository vendorRepository,
        IUnitOfWork unitOfWork,
        CreateVendorValidator validator,
        ILogger<CreateVendorCommandHandler> logger)
    {
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var vendor = new Vendor(request.Name, request.Email, request.TaxId);
        _vendorRepository.Add(vendor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created vendor {VendorId}", vendor.Id.Value);
        return vendor.Id.Value;
    }
}
