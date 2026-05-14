using Microsoft.AspNetCore.Routing;

namespace SchoolInfo.API.Endpoints;

/// <summary>
/// Minimal API endpoint'lerini gruplamak ve kaydetmek için kullanılan arayüz.
/// </summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
