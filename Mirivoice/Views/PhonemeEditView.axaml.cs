using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;

namespace Mirivoice;

public partial class PhonemeEditView : UserControl
{
    public PhonemeEditView()
    {
        InitializeComponent();

        
    }



    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}