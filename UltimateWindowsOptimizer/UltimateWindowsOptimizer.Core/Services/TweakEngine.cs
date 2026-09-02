using UltimateWindowsOptimizer.Core.Enums;
using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Core.Models;

namespace UltimateWindowsOptimizer.Core.Services;

/// <summary>
/// Production TweakEngine. All tweaks are registered at startup.
/// Adding new tweaks = implement ITweak + register. No other changes needed.
/// </summary>
public sealed class TweakEngine : ITweakEngine
{
    private readonly List<ITweak> _tweaks = new();
    private readonly Dictionary<string, ITweak> _byId = new(StringComparer.OrdinalIgnoreCase);
    private readonly IAppLogger _logger;
    private readonly IChangeHistoryService _history;
    private readonly IBackupService _backup;
    private readonly object _lock = new();

    public TweakEngine(IAppLogger logger, IChangeHistoryService history, IBackupService backup)
    {
        _logger = logger;
        _history = history;
        _backup = backup;
    }

    public IReadOnlyList<ITweak> AllTweaks
    {
        get
        {
            lock (_lock) return _tweaks.AsReadOnly();
        }
    }

    public void Register(ITweak tweak)
    {
        if (tweak == null) throw new ArgumentNullException(nameof(tweak));
        lock (_lock)
        {
            if (_byId.ContainsKey(tweak.Id))
            {
                _logger.Warning("Tweak with Id '{0}' already registered. Skipping.", tweak.Id);
                return;
            }
            _tweaks.Add(tweak);
            _byId[tweak.Id] = tweak;
            _logger.Debug("Registered tweak: {0} [{1}]", tweak.Name, tweak.Id);
        }
    }

    public void RegisterRange(IEnumerable<ITweak> tweaks)
    {
        foreach (var t in tweaks)
            Register(t);
    }

    public ITweak? GetById(string id)
    {
        lock (_lock)
            return _byId.TryGetValue(id, out var t) ? t : null;
    }

    public IEnumerable<ITweak> GetByCategory(TweakCategory category)
    {
        lock (_lock)
            return _tweaks.Where(t => t.Category == category && t.IsAvailable).ToList();
    }

    public IEnumerable<ITweak> GetByMaxRisk(RiskLevel maxRisk)
    {
        lock (_lock)
            return _tweaks.Where(t => t.RiskLevel <= maxRisk && t.IsAvailable).ToList();
    }

    public IEnumerable<ITweak> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return AllTweaks;

        var q = query.Trim();
        lock (_lock)
        {
            return _tweaks.Where(t =>
                t.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                t.Id.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                t.Category.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
    }

    public async Task<TweakResult> ApplyAsync(string tweakId, CancellationToken cancellationToken = default)
    {
        var tweak = GetById(tweakId);
        if (tweak == null)
            return TweakResult.Fail($"Tweak '{tweakId}' not found.");

        var result = await tweak.ApplyAsync(cancellationToken).ConfigureAwait(false);

        await _history.AddAsync(new ChangeRecord
        {
            TweakId = tweak.Id,
            TweakName = tweak.Name,
            Category = tweak.Category,
            OldValue = result.PreviousValue,
            NewValue = result.NewValue,
            Result = result.Result,
            Message = result.Message,
            BackupId = result.BackupId,
            CanUndo = tweak.CanUndo && result.Success
        }, cancellationToken).ConfigureAwait(false);

        return result;
    }

    public async Task<TweakResult> UndoAsync(string tweakId, CancellationToken cancellationToken = default)
    {
        var tweak = GetById(tweakId);
        if (tweak == null)
            return TweakResult.Fail($"Tweak '{tweakId}' not found.");

        var result = await tweak.UndoAsync(cancellationToken).ConfigureAwait(false);

        await _history.AddAsync(new ChangeRecord
        {
            TweakId = tweak.Id,
            TweakName = tweak.Name + " (Undo)",
            Category = tweak.Category,
            OldValue = result.PreviousValue,
            NewValue = result.NewValue,
            Result = result.Result,
            Message = result.Message,
            CanUndo = false
        }, cancellationToken).ConfigureAwait(false);

        return result;
    }

    public async Task<IReadOnlyList<TweakResult>> ApplyManyAsync(
        IEnumerable<string> tweakIds,
        IProgress<TweakProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var ids = tweakIds.ToList();
        var results = new List<TweakResult>();
        int i = 0;

        // Create one common snapshot for the batch
        var snapshot = await _backup.CreateSnapshotAsync($"Batch apply of {ids.Count} tweaks", cancellationToken)
            .ConfigureAwait(false);

        foreach (var id in ids)
        {
            cancellationToken.ThrowIfCancellationRequested();
            i++;
            var tweak = GetById(id);
            progress?.Report(new TweakProgress
            {
                TweakId = id,
                TweakName = tweak?.Name ?? id,
                Current = i,
                Total = ids.Count,
                Status = "Applying..."
            });

            var result = await ApplyAsync(id, cancellationToken).ConfigureAwait(false);
            results.Add(result);
        }

        return results;
    }

    public async Task<SystemHealthReport> AnalyzeSystemAsync(CancellationToken cancellationToken = default)
    {
        _logger.Info("Starting system analysis...");
        var report = new SystemHealthReport();
        var recommendations = new List<OptimizationRecommendation>();
        int scoreSum = 0;
        int count = 0;

        foreach (var tweak in AllTweaks.Where(t => t.IsAvailable && t.RiskLevel <= RiskLevel.Recommended))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var applied = await tweak.IsAppliedAsync(cancellationToken).ConfigureAwait(false);
                var state = await tweak.GetCurrentStateAsync(cancellationToken).ConfigureAwait(false);

                var item = new HealthCheckItem
                {
                    Name = tweak.Name,
                    Category = tweak.Category.ToString(),
                    Status = applied ? "OK" : "Optimization available",
                    Score = applied ? 100 : 60,
                    Details = state.CurrentValue,
                    Recommendation = applied ? null : tweak.ExpectedEffect
                };
                report.Items.Add(item);
                scoreSum += item.Score;
                count++;

                if (!applied)
                {
                    recommendations.Add(new OptimizationRecommendation
                    {
                        TweakId = tweak.Id,
                        Title = tweak.Name,
                        Description = tweak.Description,
                        ExpectedEffect = tweak.ExpectedEffect,
                        Risk = tweak.RiskDescription,
                        RequiresRestart = tweak.RequiresRestart,
                        CanUndo = tweak.CanUndo,
                        Priority = tweak.RiskLevel == RiskLevel.Safe ? 100 : 50
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Warning("Analysis failed for {0}: {1}", tweak.Id, ex.Message);
            }
        }

        report.OverallScore = count > 0 ? scoreSum / count : 100;
        report.PerformanceScore = report.OverallScore; // simplified
        report.SecurityScore = 85;
        report.ConfigurationScore = report.OverallScore;
        report.Recommendations = recommendations
            .OrderByDescending(r => r.Priority)
            .ToList();

        _logger.Info("System analysis complete. Score: {0}/100, Recommendations: {1}",
            report.OverallScore, report.Recommendations.Count);

        return report;
    }
}
