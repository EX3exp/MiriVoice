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

    public SingleLineEditorView(LineBoxView lineBoxView, bool FirstUpdate = true)
    {
        this.l = lineBoxView;
        InitializeComponent(l);
        
        this.pointerExit = false;
        l.DeActivatePhonemizer = false;
        this.FirstUpdate = FirstUpdate;
    }

    bool ShouldPhonemizeWhenOutFocused = false;

    private async void LineTextChanging(object sender, TextChangingEventArgs e)
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
            ShouldPhonemizeWhenOutFocused = true;
        }
        if (l.ShouldPhonemize && !l.DeActivatePhonemizer)
        {
            ShouldPhonemizeWhenOutFocused = true;
        }
        l.viewModel.LineText = viewModel.mTextBoxEditor.CurrentScript;

    }

    private void LineLostFocus(object sender, RoutedEventArgs e)
    {
        l.viewModel.LineText = viewModel.mTextBoxEditor.CurrentScript;
        //Log.Information("SingleLineTBox Lost Focus");
        //Log.Debug($"lastPhonemizedText={l.lastPhonemizedText} // LineText={l.viewModel.LineText}");
        if (l.lastPhonemizedText != l.viewModel.LineText)
        {
            //Log.Debug("lastPhonemizedText != l.viewModel.LineText");
            l.DeActivatePhonemizer = false;
            ShouldPhonemizeWhenOutFocused = true;
            
        }

        if (ShouldPhonemizeWhenOutFocused)
        {
            //Log.Debug("ShouldPhonemizeWhenOutFocused");
            l.ShouldPhonemize = true;
            Task.Run(() => l.viewModel.phonemizer.PhonemizeAsync(viewModel.mTextBoxEditor.CurrentScript, l));
            ShouldPhonemizeWhenOutFocused = false;
        }

        if (FirstUpdate)
        {
            FirstUpdate = false;
            Task.Run(() => l.viewModel.phonemizer.PhonemizeAsync(viewModel.mTextBoxEditor.CurrentScript, l));
            return;
        }
        

        if (! l.DeActivatePhonemizer || l.MResultsCollection.Count == 0 && !string.IsNullOrEmpty(l.viewModel.LineText))
        {
            l.ShouldPhonemize = true;
            return;
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