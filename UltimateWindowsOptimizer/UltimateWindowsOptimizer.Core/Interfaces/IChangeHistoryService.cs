using UltimateWindowsOptimizer.Core.Models;

namespace UltimateWindowsOptimizer.Core.Interfaces;

public interface IChangeHistoryService
{
    Task AddAsync(ChangeRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChangeRecord>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChangeRecord>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChangeRecord>> GetUndoableAsync(CancellationToken cancellationToken = default);
    Task MarkAsUndoneAsync(Guid recordId, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
    Task ExportAsync(string filePath, CancellationToken cancellationToken = default);
}
