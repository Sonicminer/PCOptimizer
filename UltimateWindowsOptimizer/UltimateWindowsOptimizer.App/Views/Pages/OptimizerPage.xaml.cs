using System.Windows;
using System.Windows.Controls;
using UltimateWindowsOptimizer.Core.Interfaces;

namespace UltimateWindowsOptimizer.App.Views.Pages;

public partial class OptimizerPage : UserControl
{
    public OptimizerPage()
    {
        InitializeComponent();
        Loaded += (_, _) => LoadTweaks();
    }

    private void LoadTweaks(string? filter = null)
    {
        IEnumerable<ITweak> tweaks = string.IsNullOrWhiteSpace(filter)
            ? App.TweakEngine.AllTweaks
            : App.TweakEngine.Search(filter);

        TweakList.ItemsSource = tweaks.OrderBy(t => t.Category).ThenBy(t => t.Name).ToList();
    }

    private void Search_Changed(object sender, TextChangedEventArgs e)
        => LoadTweaks(TxtSearch.Text);

    private async void Apply_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string id)
        {
            btn.IsEnabled = false;
            var result = await App.TweakEngine.ApplyAsync(id);
            MessageBox.Show(result.Success ? result.Message : $"Error: {result.Message}", "Apply");
            btn.IsEnabled = true;
        }
    }

    private async void Undo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string id)
        {
            btn.IsEnabled = false;
            var result = await App.TweakEngine.UndoAsync(id);
            MessageBox.Show(result.Success ? result.Message : $"Error: {result.Message}", "Undo");
            btn.IsEnabled = true;
        }
    }
}
