namespace BuildingBlocks.Abstractions;

/// <summary>
/// Coordinates repository changes into a single atomic commit.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
