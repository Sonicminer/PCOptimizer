namespace UltimateWindowsOptimizer.Update.Services;

/// <summary>
/// Semantic Version comparison (MAJOR.MINOR.PATCH).
/// 1.10.0 correctly > 1.9.0 (not alphabetical).
/// </summary>
public static class VersionComparer
{
    public static int Compare(string? a, string? b)
    {
        var va = Parse(a);
        var vb = Parse(b);
        for (int i = 0; i < 3; i++)
        {
            if (va[i] != vb[i])
                return va[i].CompareTo(vb[i]);
        }
        return 0;
    }

    public static bool IsNewer(string candidate, string current)
        => Compare(candidate, current) > 0;

    public static bool IsValid(string? version)
    {
        if (string.IsNullOrWhiteSpace(version)) return false;
        var parts = version.Trim().Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 1 || parts.Length > 4) return false;
        return parts.All(p => int.TryParse(p, out var n) && n >= 0);
    }

    private static int[] Parse(string? version)
    {
        var result = new int[3];
        if (string.IsNullOrWhiteSpace(version)) return result;
        var parts = version.Trim().Split('.', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < Math.Min(3, parts.Length); i++)
        {
            if (int.TryParse(parts[i], out var n) && n >= 0)
                result[i] = n;
        }
        return result;
    }
}
