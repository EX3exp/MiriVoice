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
        if (!FirstUpdate)
        {
            l.lastPhonemizedText = l.viewModel.LineText;
            l.DeActivatePhonemizer = true;
            l.ShouldPhonemize = false;
        }
    }

    bool ShouldPhonemizeWhenOutFocused = false;

    private void LineTextChanging(object sender, TextChangingEventArgs e)
    {
        l.DeActivatePhonemizer = false;
        if (FirstUpdate)
        {
            FirstUpdate = false;
            l.DeActivatePhonemizer = false;
            l.ShouldPhonemize = true;
            return;
        }

        if (l.singleLineEditorView is not null && l.lastPhonemizedText is not null && l.singleLineEditorView.viewModel.mTextBoxEditor.CurrentScript is not null)
        {
            if (!l.lastPhonemizedText.Equals(l.singleLineEditorView.viewModel.mTextBoxEditor.CurrentScript))
            {
                ShouldPhonemizeWhenOutFocused = true;
            }
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
        if (l.singleLineEditorView is not null && l.lastPhonemizedText is not null && l.singleLineEditorView.viewModel.mTextBoxEditor.CurrentScript is not null)
        {
            if (!l.lastPhonemizedText.Equals(l.singleLineEditorView.viewModel.mTextBoxEditor.CurrentScript))
            {
                l.DeActivatePhonemizer = false;
                l.ShouldPhonemize = true;
            }
        }


        if (ShouldPhonemizeWhenOutFocused)
        {
            //Log.Debug("ShouldPhonemizeWhenOutFocused");
            l.DeActivatePhonemizer = false;
            l.ShouldPhonemize = true;
            
            ShouldPhonemizeWhenOutFocused = false;
        }

        if (FirstUpdate)
        {
            FirstUpdate = false;
            l.DeActivatePhonemizer = false;
            l.ShouldPhonemize = true;
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

    public void SetLine(string text)
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