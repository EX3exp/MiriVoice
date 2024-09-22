using System;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Mirivoice.Mirivoice.Core.Utils;
using NAudio.SoundFont;
using Serilog;
using Velopack;
using Velopack.Sources;

namespace Mirivoice;

public partial class AppUpdaterWindow : Window
{
    private UpdateManager _um;
    private UpdateInfo _update;

    public AppUpdaterWindow()
    {
        InitializeComponent();

        _um = new UpdateManager("https://api.github.com/repos/EX3exp/MiriVoice/releases");

        TextLog.Text = "Logs:\n" + "Application started...\n";
        UpdateStatus();
    }

    private async void BtnCheckUpdateClick(object sender, RoutedEventArgs e)
    {
        Working();
        try
        {
            _update = await _um.CheckForUpdatesAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking for updates");
        }
        UpdateStatus();
    }

    private async void BtnDownloadUpdateClick(object sender, RoutedEventArgs e)
    {
        Working();
        try
        {
            await _um.DownloadUpdatesAsync(_update, Progress).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error downloading updates");
        }
        UpdateStatus();
    }

    private void BtnRestartApplyClick(object sender, RoutedEventArgs e)
    {
        Log.Information("Applying updates and restarting...");
        _um.ApplyUpdatesAndRestart(_update);
    }

    private void LogUpdated(object sender, LogUpdatedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() => {
            TextLog.Text = e.Text;
            ScrollLog.ScrollToEnd();
        });
    }

    private void Progress(int percent)
    {
        Dispatcher.UIThread.InvokeAsync(() => {
            TextStatus.Text = $"Downloading ({percent}%)...";
        });
    }

    private void Working()
    {
        Log.Information("Working on update process...");
        BtnCheckUpdate.IsEnabled = false;
        BtnDownloadUpdate.IsEnabled = false;
        BtnRestartApply.IsEnabled = false;
        TextStatus.Text = "Working...";
    }

    private void UpdateStatus()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Velopack version: {VelopackRuntimeInfo.VelopackNugetVersion}");
        sb.AppendLine($"This app version: {(_um.IsInstalled ? _um.CurrentVersion : "(n/a - not installed)")}");

        if (_update != null)
        {
            sb.AppendLine($"Update available: {_update.TargetFullRelease.Version}");
            BtnDownloadUpdate.IsEnabled = true;
        }
        else
        {
            BtnDownloadUpdate.IsEnabled = false;
        }

        if (_um.IsUpdatePendingRestart)
        {
            sb.AppendLine("Update ready, pending restart to install");
            BtnRestartApply.IsEnabled = true;
        }
        else
        {
            BtnRestartApply.IsEnabled = false;
        }

        TextStatus.Text = sb.ToString();
        BtnCheckUpdate.IsEnabled = true;
    }

    void InitializeComponent()
    {
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    }
}
