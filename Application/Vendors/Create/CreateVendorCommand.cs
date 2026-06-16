using Domain.Vendors;
using MediatR;

namespace Application.Vendors.Create;

public record CreateVendorCommand(string Name, string Email, string TaxId) : IRequest<Guid>;
