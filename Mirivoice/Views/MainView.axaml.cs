using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mirivoice.Mirivoice.Core.Editor;
using Mirivoice.Mirivoice.Core;
using Mirivoice.ViewModels;
using Avalonia.Interactivity;
using Mirivoice.Commands;

namespace Mirivoice.Views;

public partial class MainView : UserControl
{
    private MainViewModel viewModel;
    public MainView(MainViewModel v)
    {
        InitializeComponent(v);
    }

    private void OnAddButtonClick(object sender, RoutedEventArgs e)
    {
        AddLineBoxCommand addLineBoxCommand = new AddLineBoxCommand(viewModel);
        MainManager.Instance.DefaultVoicerIndex = viewModel.voicerSelector.CurrentDefaultVoicerIndex;
        MainManager.Instance.DefaultMetaIndex = viewModel.voicerSelector.Voicers[viewModel.voicerSelector.CurrentDefaultVoicerIndex]
            .VoicerMetaCollection.IndexOf(viewModel.voicerSelector.Voicers[viewModel.voicerSelector.CurrentDefaultVoicerIndex].CurrentVoicerMeta);
        MainManager.Instance.cmd.ExecuteCommand(addLineBoxCommand);
    }

    private void OnAddBoxesButtonClick(object sender, RoutedEventArgs e)
    {
        AddLineBoxesCommand addLineBoxesCommand = new AddLineBoxesCommand(viewModel);
        MainManager.Instance.DefaultVoicerIndex = viewModel.voicerSelector.CurrentDefaultVoicerIndex;
        MainManager.Instance.DefaultMetaIndex = viewModel.voicerSelector.Voicers[viewModel.voicerSelector.CurrentDefaultVoicerIndex]
            .VoicerMetaCollection.IndexOf(viewModel.voicerSelector.Voicers[viewModel.voicerSelector.CurrentDefaultVoicerIndex].CurrentVoicerMeta);

        MainManager.Instance.cmd.ExecuteCommand(addLineBoxesCommand);
    }
    private void InitializeComponent(MainViewModel v)
    {
        this.DataContext = viewModel = v;
        AvaloniaXamlLoader.Load(this);

    }


    private void InitializeComponent()
    {

        AvaloniaXamlLoader.Load(this);

    }
    

}
