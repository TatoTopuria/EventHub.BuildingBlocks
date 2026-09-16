namespace BuildingBlocks.Abstractions.Observability;

public interface ICorrelationContextAccessor
{
    string? CorrelationId { get; set; }
}
