namespace CampusRouteLab.Services;

public class RequestContextService : IRequestContextService
{
    public Guid RequestId { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.Now;
}