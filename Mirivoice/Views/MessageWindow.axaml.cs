using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Mirivoice;

public partial class MessageWindow : Window
{
    MessageWindowViewModel viewModel;
    Task taskBeforeClose;

    public bool DialogResult { get; set; }
    public MessageWindow(string message)
    {
        InitializeComponent();
        viewModel.Message = message;
        Buttons = this.FindControl<StackPanel>("Buttons");
    }


    public enum MessageBoxButtons { Ok, OkCancel, YesNo, YesNoCancel, YesNoCancelHideCancel}
    public enum MessageBoxResult { Ok, Cancel, Yes, No }

    public static Task<MessageBoxResult> Show(Window parent, string text, string title, MessageBoxButtons buttons)
    {
        var msgbox = new MessageWindow(text);
        msgbox.IsVisible = false;

        var res = MessageBoxResult.Ok;

        void AddButton(string caption, MessageBoxResult r, bool def = false)
        {
            var btn = new Button { Content = caption };
            btn.Click += (_, __) => {
                res = r;
                msgbox.Close();
            };
            msgbox.Buttons.Children.Add(btn);
            if (def)
            {
                res = r;
            }
                
        }

        if (buttons == MessageBoxButtons.Ok || buttons == MessageBoxButtons.OkCancel)
        {
            AddButton((string)parent.FindResource("app.ok"), MessageBoxResult.Ok, true);
        }
            
        if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel)
        {
            AddButton((string)parent.FindResource("app.yes"), MessageBoxResult.Yes);
            AddButton((string)parent.FindResource("app.no"), MessageBoxResult.No, false);
        }

        if (buttons == MessageBoxButtons.OkCancel || buttons == MessageBoxButtons.YesNoCancel)
        {
            AddButton((string)parent.FindResource("app.cancel"), MessageBoxResult.Cancel, true);
        }
         

        var tcs = new TaskCompletionSource<MessageBoxResult>();
        msgbox.Closed += delegate { tcs.TrySetResult(res); };
        if (parent != null)
        {
            msgbox.ShowDialog(parent);
        }
        else
        {
            msgbox.Show();
        }
        
        return tcs.Task;
    }



    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = viewModel = new MessageWindowViewModel();
    }
}