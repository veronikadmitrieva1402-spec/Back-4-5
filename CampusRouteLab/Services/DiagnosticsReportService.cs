namespace CampusRouteLab.Services;

public class DiagnosticsReportService
{
    private readonly IAppInfoService _appInfo;
    private readonly IRequestContextService _requestContext;
    private readonly ITransientMarkerService _transientMarker;
    
    public DiagnosticsReportService(
        IAppInfoService appInfo,
        IRequestContextService requestContext,
        ITransientMarkerService transientMarker)
    {
        _appInfo = appInfo;
        _requestContext = requestContext;
        _transientMarker = transientMarker;
    }
    
    public object GetReport() => new
    {
        App = new
        {
            InstanceId = _appInfo.AppInstanceId,
            StartedAt = _appInfo.StartedAt
        },
        Request = new
        {
            RequestId = _requestContext.RequestId,
            CreatedAt = _requestContext.CreatedAt
        },
        Transient = new
        {
            MarkerId = _transientMarker.MarkerId,
            CreatedAt = _transientMarker.CreatedAt
        },
        ReportGeneratedAt = DateTime.Now
    };
}