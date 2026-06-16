using Application.Data;
using Domain.Vendors;
using MediatR;

namespace Application.Vendors.Update;

internal sealed class UpdateVendorCommandHandler : IRequestHandler<UpdateVendorCommand>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVendorCommandHandler(IVendorRepository vendorRepository, IUnitOfWork unitOfWork)
    {
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.FindByIdAsync(request.VendorId, cancellationToken);
        if (vendor is null)
        {
            throw new VendorNotFoundException(request.VendorId);
        }

        vendor.UpdateContact(request.Name, request.Email, request.TaxId);
        _vendorRepository.Update(vendor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
