using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Mirivoice.ViewModels;
using System.IO;

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
