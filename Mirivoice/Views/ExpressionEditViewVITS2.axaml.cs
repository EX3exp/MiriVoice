using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Mirivoice.Engines;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using Org.BouncyCastle.Crmf;

namespace Mirivoice;

public partial class ExpressionEditViewVITS2 : UserControl
{
    public ExpressionEditViewModelVITS2 viewModel;
    VoicerMetaType vMeta;

    public ExpressionEditViewVITS2(LineBoxView l)
    {
        this.vMeta = vMeta;
        InitializeComponent(l);


    }


    private void ClearVITS2Speed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            viewModel.ClrVITS2Speed();
        }
            
    }

    private void ClearVITS2Noise1(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            viewModel.ClrVITS2Noise1();
        }
    }

    private void ClearVITS2Noise2(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            viewModel.ClrVITS2Noise2();
        }
    }
    private void InitializeComponent(LineBoxView l)
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = viewModel = new ExpressionEditViewModelVITS2(l);
    }

}