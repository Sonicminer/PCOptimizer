namespace UltimateWindowsOptimizer.Core.Models;

public class SystemHealthReport
{
    public int OverallScore { get; set; }
    public int PerformanceScore { get; set; }
    public int SecurityScore { get; set; }
    public int ConfigurationScore { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public List<HealthCheckItem> Items { get; set; } = new();
    public List<OptimizationRecommendation> Recommendations { get; set; } = new();
}

public class HealthCheckItem
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = "OK"; // OK, Warning, Critical
    public int Score { get; set; }
    public string? Details { get; set; }
    public string? Recommendation { get; set; }
}

public class OptimizationRecommendation
{
    public string TweakId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ExpectedEffect { get; set; } = string.Empty;
    public string Risk { get; set; } = string.Empty;
    public bool RequiresRestart { get; set; }
    public bool CanUndo { get; set; }
    public int Priority { get; set; } // higher = more important
    public bool IsSelected { get; set; } = true;
}
