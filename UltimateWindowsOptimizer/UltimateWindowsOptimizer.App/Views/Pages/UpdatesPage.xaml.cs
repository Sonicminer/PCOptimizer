using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using UltimateWindowsOptimizer.Update.Models;
using UltimateWindowsOptimizer.Update.Services;

namespace UltimateWindowsOptimizer.App.Views.Pages;

public partial class UpdatesPage : UserControl
{
    private readonly UpdateService _updateService;
    private UpdateManifest? _pendingManifest;

    public UpdatesPage()
    {
        InitializeComponent();
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
        _updateService = new UpdateService(version);
        TxtCurrentVersion.Text = version;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var settings = _updateService.LoadSettings();
        ChkAutoCheck.IsChecked = settings.AutomaticallyCheckForUpdates;
        ChkNotify.IsChecked = settings.NotifyAboutUpdates;
        ChkAutoDownload.IsChecked = settings.AutomaticallyDownloadUpdates;
        ChkAutoInstall.IsChecked = settings.AutomaticallyInstallUpdates;
        CmbChannel.SelectedIndex = (int)settings.Channel;
        HistoryList.ItemsSource = _updateService.LoadHistory();
    }

    private async void Check_Click(object sender, RoutedEventArgs e)
    {
        TxtStatus.Text = "Checking for updates...";
        BtnUpdateNow.Visibility = Visibility.Collapsed;
        BtnRemind.Visibility = Visibility.Collapsed;
        NotesPanel.Visibility = Visibility.Collapsed;

        var channel = (UpdateChannel)CmbChannel.SelectedIndex;
        var result = await _updateService.CheckForUpdatesAsync(channel);

        if (!result.Success)
        {
            TxtStatus.Text = result.ErrorMessage ?? "Check failed.";
            TxtLatestVersion.Text = "—";
            return;
        }

        TxtLatestVersion.Text = result.LatestVersion ?? "—";

        if (!result.UpdateAvailable)
        {
            TxtStatus.Text = "You are up to date.";
            return;
        }

        TxtStatus.Text = result.IsMandatory
            ? "A mandatory update is available."
            : "New update available.";
        BtnUpdateNow.Visibility = Visibility.Visible;
        BtnRemind.Visibility = Visibility.Visible;
        _pendingManifest = result.Manifest;

        if (result.Manifest != null && !string.IsNullOrWhiteSpace(result.Manifest.ReleaseNotes))
        {
            TxtReleaseNotes.Text = result.Manifest.ReleaseNotes;
            NotesPanel.Visibility = Visibility.Visible;
        }
    }

    private void UpdateNow_Click(object sender, RoutedEventArgs e)
    {
        if (_pendingManifest == null) return;

        var confirm = MessageBox.Show(
            $"Download and install version {_pendingManifest.Version}?\n\nThe application will close and restart after the update.",
            "Update", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        if (!_updateService.LaunchUpdater(_pendingManifest, out var error))
        {
            MessageBox.Show(error ?? "Could not start updater.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Exit so updater can replace files
        Application.Current.Shutdown();
    }

    private void Remind_Click(object sender, RoutedEventArgs e)
    {
        if (_pendingManifest != null)
        {
            var settings = _updateService.LoadSettings();
            settings.SkippedVersion = _pendingManifest.Version;
            _updateService.SaveSettings(settings);
        }
        TxtStatus.Text = "You will be reminded later.";
        BtnUpdateNow.Visibility = Visibility.Collapsed;
        BtnRemind.Visibility = Visibility.Collapsed;
    }

    private void Channel_Changed(object sender, SelectionChangedEventArgs e)
    {
        // applied on next check / save
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        var settings = _updateService.LoadSettings();
        settings.AutomaticallyCheckForUpdates = ChkAutoCheck.IsChecked == true;
        settings.NotifyAboutUpdates = ChkNotify.IsChecked == true;
        settings.AutomaticallyDownloadUpdates = ChkAutoDownload.IsChecked == true;
        settings.AutomaticallyInstallUpdates = ChkAutoInstall.IsChecked == true;
        settings.Channel = (UpdateChannel)CmbChannel.SelectedIndex;
        _updateService.SaveSettings(settings);
        MessageBox.Show("Settings saved.", "Updates");
    }
}
