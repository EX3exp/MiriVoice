using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mirivoice.ViewModels;
using System.Linq;
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


    public enum MessageBoxButtons { Ok, OkWithProgress, OkCancel, YesNo, YesNoCancel, YesNoCancelHideCancel}
    public enum MessageBoxResult { Ok, Cancel, Yes, No }

    public static async Task<MessageBoxResult> Show(Window parent, string text, string title, MessageBoxButtons buttons, Task<bool> taskBeforeClose=null, string taskProcessingMessage="", string taskEndMessage="", string taskFailedMessage="")
    {
        var msgbox = new MessageWindow(text);
        msgbox.IsVisible = false;

        var res = MessageBoxResult.Ok;

        void AddButton(string caption, MessageBoxResult r, bool def = false, bool visible=true, string name="")
        {
            var btn = new Button { 
                Content = caption,
                IsVisible = visible
            };


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

        if (buttons == MessageBoxButtons.OkWithProgress)
        {
            AddButton((string)parent.FindResource("app.ok"), MessageBoxResult.Ok, true, false, "okButton");
            
            msgbox.viewModel.Message = taskProcessingMessage;
            var result = await Task.Run(() => taskBeforeClose);
            if (result)
            {
                msgbox.viewModel.Message = taskEndMessage;
            }
            else
            {
                msgbox.viewModel.Message = taskFailedMessage;
            }

            msgbox.Buttons.Children.First().IsVisible = true;
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
        
        return await tcs.Task;
    }



    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = viewModel = new MessageWindowViewModel();
    }
}