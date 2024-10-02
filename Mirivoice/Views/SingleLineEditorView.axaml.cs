using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using Serilog;
using System.Threading.Tasks;

namespace Mirivoice;

public partial class SingleLineEditorView : UserControl
{
    private readonly LineBoxView l;
    private bool pointerExit;
    public SingleLineEditorViewModel viewModel;
    bool FirstUpdate = false;

    public SingleLineEditorView(LineBoxView l, bool FirstUpdate = true)
    {
        InitializeComponent(l);
        this.l = l;
        this.pointerExit = false;
        l.DeActivatePhonemizer = false;
        this.FirstUpdate = FirstUpdate;
    }


    private async void LineTextChanged(object sender, TextChangedEventArgs e)
    {
        
        l.DeActivatePhonemizer = false;
        if (FirstUpdate)
        {
            FirstUpdate = false;
            Task.Run(() => l.viewModel.phonemizer.PhonemizeAsync(viewModel.mTextBoxEditor.CurrentScript, l));
            return;
        }
        if (l.lastPhonemizedText != l.viewModel.LineText)
        {
            l.ShouldPhonemize = true;
        }
        if (l.ShouldPhonemize && !l.DeActivatePhonemizer)
        {
            await Task.Run(() => l.viewModel.phonemizer.PhonemizeAsync(viewModel.mTextBoxEditor.CurrentScript, l));
        }
        l.viewModel.LineText = viewModel.mTextBoxEditor.CurrentScript;

    }

    private void LineLostFocus(object sender, RoutedEventArgs e)
    {
        //Log.Debug("SingleLineTBox Lost Focus");

        if (FirstUpdate)
        {
            FirstUpdate = false;
            Task.Run(() => l.viewModel.phonemizer.PhonemizeAsync(viewModel.mTextBoxEditor.CurrentScript, l));
            return;
        }


        if (! l.DeActivatePhonemizer || l.MResultsCollection.Count == 0 && !string.IsNullOrEmpty(l.viewModel.LineText))
        {
            l.ShouldPhonemize = true;

        }

        
    }   

    private void LineGotFocus(object sender, GotFocusEventArgs e)
    {
        //Log.Debug("SingleLineTBox Got Focus");
        l.ShouldPhonemize = false;
    }

    private void InitializeComponent(LineBoxView l)
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = viewModel = new SingleLineEditorViewModel(l.viewModel.LineText);
    }

    public async void SetLine(string text)
    {
        if (text == null)
        {
            text = string.Empty;
            return;
        }

        viewModel.mTextBoxEditor.CurrentScript = text;
        l.viewModel.LineText = text;
    }

}