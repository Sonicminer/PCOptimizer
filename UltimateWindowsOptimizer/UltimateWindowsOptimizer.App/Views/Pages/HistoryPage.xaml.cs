using System.Windows.Controls;

namespace UltimateWindowsOptimizer.App.Views.Pages;

public partial class HistoryPage : UserControl
{
    public HistoryPage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            var items = await App.HistoryService.GetRecentAsync(100);
            HistoryList.ItemsSource = items;
        };
    }
}
