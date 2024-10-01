using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Serilog;
using System.Threading.Tasks;
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
            TaskScheduler.FromCurrentSynchronizationContext(), this);
        Log.Information("Created main window.");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}