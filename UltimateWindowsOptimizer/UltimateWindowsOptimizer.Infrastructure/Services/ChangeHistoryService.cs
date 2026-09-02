using System.Text.Json;
using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Core.Models;

namespace UltimateWindowsOptimizer.Infrastructure.Services;

public sealed class ChangeHistoryService : IChangeHistoryService
{
    private readonly string _historyFile;
    private readonly IAppLogger _logger;
    private readonly List<ChangeRecord> _cache = new();
    private readonly object _lock = new();
    private readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    public ChangeHistoryService(IAppLogger logger, string? dataDir = null)
    {
        _logger = logger;
        var root = dataDir ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UltimateWindowsOptimizer");
        Directory.CreateDirectory(root);
        _historyFile = Path.Combine(root, "change_history.json");
        Load();
    }

    public Task AddAsync(ChangeRecord record, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _cache.Add(record);
            Save();
        }
        _logger.Info("Change recorded: {0} -> {1}", record.TweakName, record.Result);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ChangeRecord>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        lock (_lock)
            return Task.FromResult<IReadOnlyList<ChangeRecord>>(
                _cache.OrderByDescending(c => c.Timestamp).Take(count).ToList());
    }

    public Task<IReadOnlyList<ChangeRecord>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        lock (_lock)
            return Task.FromResult<IReadOnlyList<ChangeRecord>>(
                _cache.Where(c => c.Timestamp.Date == date.Date).ToList());
    }

    public Task<IReadOnlyList<ChangeRecord>> GetUndoableAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
            return Task.FromResult<IReadOnlyList<ChangeRecord>>(
                _cache.Where(c => c.CanUndo && !c.WasUndone).OrderByDescending(c => c.Timestamp).ToList());
    }

    public Task MarkAsUndoneAsync(Guid recordId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var rec = _cache.FirstOrDefault(c => c.Id == recordId);
            if (rec != null)
            {
                rec.WasUndone = true;
                Save();
            }
        }
        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _cache.Clear();
            Save();
        }
        return Task.CompletedTask;
    }

    public async Task ExportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        string json;
        lock (_lock)
            json = JsonSerializer.Serialize(_cache, _json);
        await File.WriteAllTextAsync(filePath, json, cancellationToken).ConfigureAwait(false);
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_historyFile)) return;
            var json = File.ReadAllText(_historyFile);
            var items = JsonSerializer.Deserialize<List<ChangeRecord>>(json);
            if (items != null)
            {
                lock (_lock) _cache.AddRange(items);
            }
        }
        catch (Exception ex)
        {
            _logger.Warning("Could not load change history: {0}", ex.Message);
        }
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_cache, _json);
            File.WriteAllText(_historyFile, json);
        }
        catch (Exception ex)
        {
            _logger.Warning("Could not save change history: {0}", ex.Message);
        }
    }
}
