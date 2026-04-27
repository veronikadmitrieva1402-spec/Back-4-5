namespace CampusRouteLab.Middleware;

public static class RequestAuditExtensions
{
    public static IApplicationBuilder UseRequestAudit(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestAuditMiddleware>();
    }
}