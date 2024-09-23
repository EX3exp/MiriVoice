using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mirivoice.ViewModels;
using Serilog;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
namespace Mirivoice.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Log.Information("Main window checking Update.");
        AppUpdaterWindow.CheckForUpdate(
            dialog => dialog.Show(this),
            () => (Application.Current?.ApplicationLifetime as IControlledApplicationLifetime)?.Shutdown(),
            TaskScheduler.FromCurrentSynchronizationContext());
        Log.Information("Created main window.");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}