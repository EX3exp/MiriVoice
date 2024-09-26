using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mirivoice.ViewModels;
using Mirivoice.Mirivoice.Core.Format;
namespace Mirivoice;

public partial class VoicersVoicerButton : UserControl
{
    public VoicersVoicerButtonViewModel viewModel;
    public VoicersVoicerButton(Voicer voicer, VoicersWindowViewModel v, MainViewModel mv)
    {
        
        InitializeComponent(voicer, v, mv);
        
    }

    private void InitializeComponent(Voicer voicer, VoicersWindowViewModel v, MainViewModel mv)
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = viewModel = new VoicersVoicerButtonViewModel(voicer, v, mv);
    }
}