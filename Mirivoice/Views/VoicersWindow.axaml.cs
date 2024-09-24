using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mirivoice.ViewModels;

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
}