using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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