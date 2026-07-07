namespace Cookbook.Domain.Common;

public abstract record DomainEvent(DateTime OccurredOn)
{
    protected DomainEvent() : this(DateTime.UtcNow)
    {
    }
}