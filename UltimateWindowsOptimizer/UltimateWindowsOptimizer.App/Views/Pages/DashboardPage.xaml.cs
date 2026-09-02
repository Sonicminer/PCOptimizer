using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using UltimateWindowsOptimizer.Core.Models;

namespace UltimateWindowsOptimizer.App.Views.Pages;

public partial class DashboardPage : UserControl
{
    public DashboardPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await RefreshMetrics();
    }

    private async Task RefreshMetrics()
    {
        try
        {
            // Simple process-based metrics (real implementation uses PerformanceCounter / LibreHardwareMonitor)
            var ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            TxtRam.Text = $"{ramCounter.NextValue():F0} %";

            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            TxtUptime.Text = $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
            TxtCpu.Text = "—";
            TxtDisk.Text = "—";
        }
        catch
        {
            TxtCpu.Text = "N/A";
            TxtRam.Text = "N/A";
        }
        await Task.CompletedTask;
    }

    private async void Analyze_Click(object sender, RoutedEventArgs e)
    {
        TxtAnalysis.Text = "Analyzing system...";
        LstRecommendations.Items.Clear();

        try
        {
            var report = await App.TweakEngine.AnalyzeSystemAsync();
            TxtAnalysis.Text = $"SYSTEM HEALTH: {report.OverallScore}/100\n" +
                               $"Performance: {report.PerformanceScore} | Security: {report.SecurityScore} | Config: {report.ConfigurationScore}\n" +
                               $"Tweaks analyzed: {report.Items.Count} | Recommendations: {report.Recommendations.Count}";

            foreach (var rec in report.Recommendations)
            {
                var cb = new CheckBox
                {
                    Content = $"[{(rec.IsSelected ? "✓" : " ")}] {rec.Title}  –  {rec.ExpectedEffect}",
                    IsChecked = rec.IsSelected,
                    Margin = new Thickness(0, 4, 0, 4),
                    Tag = rec
                };
                LstRecommendations.Items.Add(cb);
            }
        }
        catch (Exception ex)
        {
            TxtAnalysis.Text = "Analysis failed: " + ex.Message;
        }
    }

    private async void OneClick_Click(object sender, RoutedEventArgs e)
    {
        var report = await App.TweakEngine.AnalyzeSystemAsync();
        var ids = report.Recommendations
            .Where(r => r.IsSelected)
            .Select(r => r.TweakId)
            .ToList();

        if (ids.Count == 0)
        {
            MessageBox.Show("No recommendations available or everything already optimized.", "1-Click");
            return;
        }

        var confirm = MessageBox.Show(
            $"Apply {ids.Count} recommended optimizations?\nA backup will be created first.",
            "1-Click Optimization", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        var results = await App.TweakEngine.ApplyManyAsync(ids);
        var ok = results.Count(r => r.Success);
        MessageBox.Show($"Done. {ok}/{results.Count} succeeded.", "1-Click");
        Analyze_Click(sender, e);
    }

    private void Recommendations_Click(object sender, RoutedEventArgs e) => Analyze_Click(sender, e);
}
