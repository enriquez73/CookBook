namespace Cookbook.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; protected set; }

    public DateTime? UpdatedAt { get; protected set; }

    public string? CreatedBy { get; protected set; }

    public string? UpdatedBy { get; protected set; }

    protected AuditableEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsUpdated(string? user = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = user;
    }
}