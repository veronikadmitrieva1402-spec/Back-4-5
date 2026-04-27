using CampusRouteLab.Services;

namespace CampusRouteLab.Middleware;

public class RequestAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAppInfoService _appInfoService;

    public RequestAuditMiddleware(RequestDelegate next, IAppInfoService appInfoService)
    {
        _next = next;
        _appInfoService = appInfoService;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IRequestContextService requestContextService,
        ITransientMarkerService transientMarkerService)
    {
        var startTime = DateTime.Now;
        
        if (context.Request.Path.StartsWithSegments("/diag"))
        {
            Console.WriteLine($"[DIAG] Request to: {context.Request.Path}");
            Console.WriteLine($"[DIAG] Scoped RequestId: {requestContextService.RequestId}");
            Console.WriteLine($"[DIAG] Transient MarkerId: {transientMarkerService.MarkerId}");
        }
        
        await _next(context);
        
        context.Response.Headers.Append("X-App-Instance", _appInfoService.AppInstanceId.ToString());
        context.Response.Headers.Append("X-Request-Id", requestContextService.RequestId.ToString());
        context.Response.Headers.Append("X-Transient-Id", transientMarkerService.MarkerId.ToString());
        context.Response.Headers.Append("X-Processing-Time-Ms", (DateTime.Now - startTime).TotalMilliseconds.ToString());
    }
}