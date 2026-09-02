using System.Collections.Concurrent;
using UltimateWindowsOptimizer.Core.Interfaces;

namespace UltimateWindowsOptimizer.Infrastructure.Logging;

public sealed class FileLogger : IAppLogger
{
    private readonly string _logDirectory;
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly object _writeLock = new();
    private bool _developerMode;

    public FileLogger(string? logDirectory = null)
    {
        _logDirectory = logDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UltimateWindowsOptimizer", "Logs");
        Directory.CreateDirectory(_logDirectory);
    }

    public bool DeveloperMode
    {
        get => _developerMode;
        set => _developerMode = value;
    }

    public void Debug(string message, params object[] args) => Write("DEBUG", message, args);
    public void Info(string message, params object[] args) => Write("INFO ", message, args);
    public void Warning(string message, params object[] args) => Write("WARN ", message, args);
    public void Error(string message, Exception? ex = null, params object[] args)
    {
        Write("ERROR", message, args);
        if (ex != null)
            Write("ERROR", ex.ToString());
    }
    public void Critical(string message, Exception? ex = null, params object[] args)
    {
        Write("CRIT ", message, args);
        if (ex != null)
            Write("CRIT ", ex.ToString());
    }

    public void Trace(string category, string message, object? data = null)
    {
        if (!_developerMode) return;
        var dataStr = data != null ? " | " + data : "";
        Write("TRACE", $"[{category}] {message}{dataStr}");
    }

    public IDisposable BeginScope(string scopeName) => new LogScope(this, scopeName);

    private void Write(string level, string message, params object[] args)
    {
        try
        {
            var formatted = args.Length > 0 ? string.Format(message, args) : message;
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {formatted}";
            lock (_writeLock)
            {
                var file = Path.Combine(_logDirectory, $"optimizer_{DateTime.Now:yyyyMMdd}.log");
                File.AppendAllText(file, line + Environment.NewLine);
            }
        }
        catch
        {
            // never crash the app because of logging
        }
    }

    private sealed class LogScope : IDisposable
    {
        private readonly FileLogger _logger;
        private readonly string _name;
        public LogScope(FileLogger logger, string name)
        {
            _logger = logger;
            _name = name;
            _logger.Debug(">>> Enter scope: {0}", name);
        }
        public void Dispose() => _logger.Debug("<<< Leave scope: {0}", _name);
    }
}
