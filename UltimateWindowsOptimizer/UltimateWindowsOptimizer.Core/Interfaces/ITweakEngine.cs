using UltimateWindowsOptimizer.Core.Enums;
using UltimateWindowsOptimizer.Core.Models;

namespace UltimateWindowsOptimizer.Core.Interfaces;

/// <summary>
/// Central engine that discovers, manages and executes all tweaks.
/// Designed for easy extension: just register new ITweak implementations.
/// </summary>
public interface ITweakEngine
{
    /// <summary>All registered tweaks</summary>
    IReadOnlyList<ITweak> AllTweaks { get; }

    /// <summary>Get tweaks filtered by category</summary>
    IEnumerable<ITweak> GetByCategory(TweakCategory category);

    /// <summary>Get tweaks by risk level (and below)</summary>
    IEnumerable<ITweak> GetByMaxRisk(RiskLevel maxRisk);

    /// <summary>Find a tweak by its stable Id</summary>
    ITweak? GetById(string id);

    /// <summary>Search tweaks by name/description</summary>
    IEnumerable<ITweak> Search(string query);

    /// <summary>Register a new tweak (used by modules)</summary>
    void Register(ITweak tweak);

    /// <summary>Register many tweaks at once</summary>
    void RegisterRange(IEnumerable<ITweak> tweaks);

    /// <summary>Apply a single tweak with full safety pipeline</summary>
    Task<TweakResult> ApplyAsync(string tweakId, CancellationToken cancellationToken = default);

    /// <summary>Undo a single tweak</summary>
    Task<TweakResult> UndoAsync(string tweakId, CancellationToken cancellationToken = default);

    /// <summary>Apply multiple selected recommendations</summary>
    Task<IReadOnlyList<TweakResult>> ApplyManyAsync(IEnumerable<string> tweakIds, IProgress<TweakProgress>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>Analyze system and produce recommendations</summary>
    Task<SystemHealthReport> AnalyzeSystemAsync(CancellationToken cancellationToken = default);
}

public class TweakProgress
{
    public string TweakId { get; set; } = string.Empty;
    public string TweakName { get; set; } = string.Empty;
    public int Current { get; set; }
    public int Total { get; set; }
    public string Status { get; set; } = string.Empty;
}
