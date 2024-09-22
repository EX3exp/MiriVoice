using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mirivoice.ViewModels;

namespace Mirivoice;

public partial class VoicersWindow : Window
{
    // todo
    public VoicersWindow(MainViewModel v, Window owner)
    {
        owner.Closed += (_, __) => Close();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}