using UltimateWindowsOptimizer.Core.Enums;

namespace UltimateWindowsOptimizer.Core.Models;

public class TweakResult
{
    public bool Success { get; set; }
    public ChangeResult Result { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public bool RestartRequired { get; set; }
    public string? BackupId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Exception? Exception { get; set; }

    public static TweakResult Ok(string message, string? previous = null, string? @new = null, string? backupId = null)
        => new()
        {
            Success = true,
            Result = ChangeResult.Success,
            Message = message,
            PreviousValue = previous,
            NewValue = @new,
            BackupId = backupId
        };

    public static TweakResult Fail(string message, string? details = null, Exception? ex = null)
        => new()
        {
            Success = false,
            Result = ChangeResult.Failed,
            Message = message,
            ErrorDetails = details,
            Exception = ex
        };

    public static TweakResult RequiresRestart(string message, string? previous = null, string? @new = null)
        => new()
        {
            Success = true,
            Result = ChangeResult.RequiresRestart,
            Message = message,
            PreviousValue = previous,
            NewValue = @new,
            RestartRequired = true
        };
}
