using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mirivoice.ViewModels;

namespace Mirivoice.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}