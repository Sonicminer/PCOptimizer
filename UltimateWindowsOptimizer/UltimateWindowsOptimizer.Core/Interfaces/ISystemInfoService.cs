using UltimateWindowsOptimizer.Core.Models;

namespace UltimateWindowsOptimizer.Core.Interfaces;

public interface ISystemInfoService
{
    Task<SystemInfo> GetSystemInfoAsync(CancellationToken cancellationToken = default);
    Task<HardwareInfo> GetHardwareInfoAsync(CancellationToken cancellationToken = default);
    Task<PerformanceMetrics> GetLiveMetricsAsync(CancellationToken cancellationToken = default);
    string GetWindowsVersion();
    bool IsWindows11();
    bool IsElevated();
}

public class SystemInfo
{
    public string ComputerName { get; set; } = string.Empty;
    public string WindowsVersion { get; set; } = string.Empty;
    public string WindowsEdition { get; set; } = string.Empty;
    public string BuildNumber { get; set; } = string.Empty;
    public string Architecture { get; set; } = string.Empty;
    public TimeSpan Uptime { get; set; }
    public DateTime BootTime { get; set; }
    public long TotalRamBytes { get; set; }
    public long AvailableRamBytes { get; set; }
    public int LogicalProcessors { get; set; }
    public string? CpuName { get; set; }
    public string? GpuName { get; set; }
}

public class HardwareInfo
{
    public CpuInfo? Cpu { get; set; }
    public List<GpuInfo> Gpus { get; set; } = new();
    public List<MemoryModule> MemoryModules { get; set; } = new();
    public List<StorageDevice> StorageDevices { get; set; } = new();
    public List<NetworkAdapterInfo> NetworkAdapters { get; set; } = new();
    public MotherboardInfo? Motherboard { get; set; }
    public List<MonitorInfo> Monitors { get; set; } = new();
}

public class CpuInfo
{
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public int Cores { get; set; }
    public int LogicalProcessors { get; set; }
    public double? CurrentClockMhz { get; set; }
    public double? MaxClockMhz { get; set; }
    public double? TemperatureCelsius { get; set; }
    public double? LoadPercent { get; set; }
}

public class GpuInfo
{
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string DriverVersion { get; set; } = string.Empty;
    public long? DedicatedMemoryBytes { get; set; }
    public double? TemperatureCelsius { get; set; }
    public double? LoadPercent { get; set; }
}

public class MemoryModule
{
    public string Manufacturer { get; set; } = string.Empty;
    public long CapacityBytes { get; set; }
    public int SpeedMhz { get; set; }
    public string FormFactor { get; set; } = string.Empty;
}

public class StorageDevice
{
    public string Model { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // SSD, HDD, NVMe
    public long SizeBytes { get; set; }
    public long FreeBytes { get; set; }
    public string? Interface { get; set; }
    public int? HealthPercent { get; set; }
    public string? SmartStatus { get; set; }
}

public class NetworkAdapterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? Gateway { get; set; }
    public string? DnsServers { get; set; }
    public bool IsConnected { get; set; }
    public long SpeedBps { get; set; }
}

public class MotherboardInfo
{
    public string Manufacturer { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string? BiosVersion { get; set; }
    public string? BiosDate { get; set; }
}

public class MonitorInfo
{
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int RefreshRate { get; set; }
}

public class PerformanceMetrics
{
    public double CpuUsagePercent { get; set; }
    public double? CpuTemperature { get; set; }
    public double RamUsagePercent { get; set; }
    public long RamUsedBytes { get; set; }
    public long RamTotalBytes { get; set; }
    public double? GpuUsagePercent { get; set; }
    public double? GpuTemperature { get; set; }
    public double DiskUsagePercent { get; set; }
    public long NetworkBytesSentPerSec { get; set; }
    public long NetworkBytesReceivedPerSec { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
