using System.Threading;
using BuildingBlocks.Abstractions.Observability;

namespace BuildingBlocks.Observability;

public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<string?> Current = new();

    public string? CorrelationId
    {
        get => Current.Value;
        set => Current.Value = value;
    }
}
