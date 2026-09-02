namespace UltimateWindowsOptimizer.Core.Models;

/// <summary>
/// Represents the current value/state of a tweak on the system.
/// </summary>
public class TweakState
{
    public string CurrentValue { get; set; } = string.Empty;
    public string? OptimizedValue { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsOptimized { get; set; }
    public bool IsDefault { get; set; }
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? AdditionalData { get; set; }
}
