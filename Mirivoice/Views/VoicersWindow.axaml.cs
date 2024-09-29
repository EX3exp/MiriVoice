using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mirivoice.ViewModels;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Mirivoice;



public partial class VoicersWindow : Window
{
    VoicersWindowViewModel viewModel;
    public VoicersWindow(MainViewModel v, Window owner)
    {
        owner.Closed += (_, __) => Close();
        InitializeComponent(v);
    }

    private void InitializeComponent(MainViewModel v)
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = viewModel = new VoicersWindowViewModel(v);

    }

    private void OnVoicerWebClicked(object sender, TappedEventArgs e)
    {
        OpenUrl(viewModel.Web);
    }

    public static void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}