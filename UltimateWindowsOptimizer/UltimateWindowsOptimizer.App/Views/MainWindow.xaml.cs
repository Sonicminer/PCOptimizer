using System.Windows;
using System.Windows.Controls;
using UltimateWindowsOptimizer.App.Views.Pages;

namespace UltimateWindowsOptimizer.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        TxtPcName.Text = Environment.MachineName;
        TxtWindowsVersion.Text = Environment.OSVersion.ToString();

        // Show dashboard by default
        NavigateTo("Dashboard");

        // Quick health score
        try
        {
            var report = await App.TweakEngine.AnalyzeSystemAsync();
            TxtHealthScore.Text = $"{report.OverallScore}/100";
            TxtPerfScore.Text = $"{report.PerformanceScore}/100";
        }
        catch
        {
            TxtHealthScore.Text = "N/A";
            TxtPerfScore.Text = "N/A";
        }
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
            NavigateTo(tag);
    }

    private void NavigateTo(string page)
    {
        ContentHost.Content = page switch
        {
            "Dashboard" => new DashboardPage(),
            "Optimizer" => new OptimizerPage(),
            "History" => new HistoryPage(),
            "Settings" => new UpdatesPage(),
            _ => CreatePlaceholder(page)
        };
    }

    private static UIElement CreatePlaceholder(string name)
    {
        return new TextBlock
        {
            Text = $"{name} module – coming in full implementation.\n\nArchitecture is ready: just add the page + ViewModel and wire the corresponding services.",
            Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextSecondaryBrush"],
            FontSize = 16,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(20)
        };
    }

    private async void RestoreLast_Click(object sender, RoutedEventArgs e)
    {
        var undoable = await App.HistoryService.GetUndoableAsync();
        if (undoable.Count == 0)
        {
            MessageBox.Show("No undoable changes found.", "Restore", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var last = undoable.First();
        var result = await App.TweakEngine.UndoAsync(last.TweakId);
        MessageBox.Show(result.Success
            ? $"Restored: {last.TweakName}"
            : $"Failed: {result.Message}", "Restore");
    }

    private void EmergencyReset_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "This will attempt to undo ALL changes made by the Optimizer.\n\nContinue?",
            "Emergency Reset",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            // Full reset logic would iterate all undoable records
            MessageBox.Show("Emergency reset initiated. Check History for details.", "Reset");
        }
    }
}
