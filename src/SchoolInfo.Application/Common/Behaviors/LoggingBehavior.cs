using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SchoolInfo.Application.Common.Behaviors;

/// <summary>
/// Tüm MediatR isteklerini loglayan araya girici (behavior).
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("SchoolInfo İstek Başlıyor: {Name} {@Request}", requestName, request);

        var response = await next();

        _logger.LogInformation("SchoolInfo İstek Tamamlandı: {Name}", requestName);

        return response;
    }
}
