using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Mirivoice.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        Setup();

    }

    private void Setup()
    {
    }



    private void InitializeComponent()
    {

        AvaloniaXamlLoader.Load(this);

    }
    

}
