using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mirivoice.ViewModels;

namespace Mirivoice;

public partial class GlobalSettingWindow : Window
{
    private GlobalSettingWindowViewModel viewModel;
    public GlobalSettingWindow(Window owner)
    {
        owner.Closed += (_, __) => Close();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = viewModel = new GlobalSettingWindowViewModel();
    }
}