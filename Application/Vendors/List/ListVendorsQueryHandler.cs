using Domain.Vendors;
using MediatR;

namespace Application.Vendors.List;

internal sealed class ListVendorsQueryHandler : IRequestHandler<ListVendorsQuery, PagedVendorResponse>
{
    private const int MaxPageSize = 100;

    private readonly IVendorRepository _vendorRepository;

    public ListVendorsQueryHandler(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<PagedVendorResponse> Handle(ListVendorsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, MaxPageSize);

        var (items, totalCount) = await _vendorRepository.ListPagedAsync(page, pageSize, cancellationToken);

        var mapped = items
            .Select(v => new VendorListItem(v.Id.Value, v.Name, v.Email, v.Status.ToString()))
            .ToList();

        return new PagedVendorResponse(mapped, totalCount, page, pageSize);
    }
}
