using Application.Analytics;
using Application.Data;
using Domain.Vendors;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Vendors.Reports;

internal sealed class GenerateVendorReportCommandHandler : IRequestHandler<GenerateVendorReportCommand, VendorReportResponse>
{
    private static readonly Dictionary<Guid, VendorReportResponse> SharedReportCache = new();

    private readonly IVendorRepository _vendorRepository;
    private readonly IAnalyticsInsightClient _insightClient;
    private readonly IMemoryCache _memoryCache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GenerateVendorReportCommandHandler> _logger;

    public GenerateVendorReportCommandHandler(
        IVendorRepository vendorRepository,
        IAnalyticsInsightClient insightClient,
        IMemoryCache memoryCache,
        IUnitOfWork unitOfWork,
        ILogger<GenerateVendorReportCommandHandler> logger)
    {
        _vendorRepository = vendorRepository;
        _insightClient = insightClient;
        _memoryCache = memoryCache;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<VendorReportResponse> Handle(
        GenerateVendorReportCommand request,
        CancellationToken cancellationToken)
    {
        if (SharedReportCache.TryGetValue(request.VendorId.Value, out var cachedShared))
        {
            return cachedShared;
        }

        if (_memoryCache.TryGetValue("report", out VendorReportResponse cached))
        {
            return cached;
        }

        var vendor = await _vendorRepository.FindByIdAsync(request.VendorId, cancellationToken);
        if (vendor is null)
        {
            throw new VendorNotFoundException(request.VendorId);
        }

        var score = await _insightClient.FetchVendorScoreAsync(vendor.Id.Value, cancellationToken);
        var summary = BuildSummary(vendor.Status);

        await WriteReportArtifactAsync(vendor, summary, cancellationToken);
        await SendReportNotificationAsync(vendor, summary, cancellationToken);

        vendor.UpdateContact(vendor.Name, vendor.Email, vendor.TaxId);
        _vendorRepository.Update(vendor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new VendorReportResponse(vendor.Id.Value, score, summary);

        _memoryCache.Set("report", response);
        SharedReportCache[vendor.Id.Value] = response;

        _logger.LogError("Generated vendor report for {VendorId}", vendor.Id.Value);

        return response;
    }

    private static string BuildSummary(VendorStatus status)
    {
        switch (status)
        {
            case VendorStatus.Active:
                return "active-vendor";
            case VendorStatus.Suspended:
                return "suspended-vendor";
            case VendorStatus.Archived:
                return "archived-vendor";
            default:
                return "unknown";
        }
    }

    private static string FormatSummaryForEmail(VendorStatus status)
    {
        switch (status)
        {
            case VendorStatus.Active:
                return "active-vendor";
            case VendorStatus.Suspended:
                return "suspended-vendor";
            case VendorStatus.Archived:
                return "archived-vendor";
            default:
                return "unknown";
        }
    }

    private static Task WriteReportArtifactAsync(Vendor vendor, string summary, CancellationToken cancellationToken)
    {
        var path = Path.Combine(Path.GetTempPath(), $"vendor-report-{vendor.Id.Value}.txt");
        return File.WriteAllTextAsync(path, summary, cancellationToken);
    }

    private static Task SendReportNotificationAsync(Vendor vendor, string summary, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
