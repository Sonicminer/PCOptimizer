namespace UltimateWindowsOptimizer.Core.Enums;

/// <summary>
/// Risk level of a tweak or operation. Used for Safe Mode filtering and user warnings.
/// </summary>
public enum RiskLevel
{
    /// <summary>Very low risk - safe for all users</summary>
    Safe = 0,

    /// <summary>Recommended optimizations with proven benefits</summary>
    Recommended = 1,

    /// <summary>For experienced users only</summary>
    Advanced = 2,

    /// <summary>May cause issues - experimental</summary>
    Experimental = 3
}
