using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mirivoice.Mirivoice.Core;
using Mirivoice.ViewModels;
using NetSparkleUpdater.Enums;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace Mirivoice;

public partial class AppUpdaterWindow : Window {
    public AppUpdaterViewModel viewModel;
    public AppUpdaterWindow(Window owner) {
        InitializeComponent(owner);
        
    }

    private void InitializeComponent(Window owner)
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = viewModel = new AppUpdaterViewModel(owner);
    }

    void OnClosing(object sender, WindowClosingEventArgs e) 
    {
        viewModel.OnClosing();
    }
    public static void CheckForUpdate(Action<Window> showDialog, Action closeApplication, TaskScheduler scheduler, Window mainWindow) {
        Task.Run(async () => {
            using var updater = await AppUpdaterViewModel.NewUpdaterAsync();
            if (updater == null) {
                return false;
            }
            var info = await updater.CheckForUpdatesQuietly(true);
            if (info.Status == UpdateStatus.UpdateAvailable) {
                if (info.Updates[0].Version.ToString() == MainManager.Instance.Setting.SkipUpdate) {
                    return false;
                }
                return true;
            }
            return false;
        }).ContinueWith(t => {
            if (t.IsCompletedSuccessfully && t.Result) {
                var dialog = new AppUpdaterWindow(mainWindow);
                dialog.viewModel.CloseApplication = closeApplication;
                    showDialog.Invoke(dialog);
                }
                if (t.IsFaulted) {
                    Log.Error(t.Exception, "Failed to check for update");
                }
            }, scheduler).ContinueWith((t2, _) => {
                if (t2.IsFaulted) {
                    Log.Error(t2.Exception, "Failed to check for update");
                }
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
