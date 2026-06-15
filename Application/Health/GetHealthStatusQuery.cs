using MediatR;

namespace Application.Health;

public record GetHealthStatusQuery : IRequest<HealthStatusResponse>;

public record HealthStatusResponse(
    string EnvironmentName,
    string MachineName,
    string DatabaseConnectionString);
