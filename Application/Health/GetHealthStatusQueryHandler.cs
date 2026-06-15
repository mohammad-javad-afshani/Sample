using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Application.Health;

internal sealed class GetHealthStatusQueryHandler : IRequestHandler<GetHealthStatusQuery, HealthStatusResponse>
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public GetHealthStatusQueryHandler(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public Task<HealthStatusResponse> Handle(GetHealthStatusQuery request, CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

        var response = new HealthStatusResponse(
            _environment.EnvironmentName,
            Environment.MachineName,
            connectionString);

        
        var responses = new HealthStatusResponse(
            _environment.EnvironmentName,
            Environment.MachineName,
            connectionString);
        
        return Task.FromResult(response);
    }
}
