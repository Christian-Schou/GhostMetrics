using System.Diagnostics;
using GhostMetrics.Core.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GhostMetrics.Core.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly IUserService _userService;
    private readonly IIdentityService _identityService;
    
    public PerformanceBehaviour(
        ILogger<TRequest> logger,
        IUserService userService,
        IIdentityService identityService)
    {
        _timer = new Stopwatch();
        _logger = logger;
        _userService = userService;
        _identityService = identityService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();
        
        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds <= 500) return response;
        
        var requestName = typeof(TRequest).Name;
        var userId = _userService.Id ?? string.Empty;
        var userName = string.Empty;

        if (!string.IsNullOrEmpty(userId))
        {
            userName = await _identityService.GetUsernameAsync(userId);
        }
            
        _logger.LogWarning("GhostMetrics Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
            requestName, elapsedMilliseconds, userId, userName, request);

        return response;
    }
}