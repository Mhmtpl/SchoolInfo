using Microsoft.AspNetCore.Routing;

namespace SchoolInfo.API.Endpoints;

/// <summary>
/// Minimal API endpoint'lerini gruplamak ve kaydetmek iÃ§in kullanÄ±lan arayÃ¼z.
/// </summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
