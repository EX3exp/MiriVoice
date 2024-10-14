using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Mirivoice.Engines;
using Mirivoice.Mirivoice.Core;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using Org.BouncyCastle.Crmf;
using ReactiveUI;
using System;
using System.ComponentModel;
using Serilog;

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

    private void OnChangedSpeed(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (viewModel is null)
            return;
        viewModel.IsDragging = false;
        viewModel.OnPropertyChanged(nameof(viewModel.VITS2Speed));
    }
    private void OnChangedNoise1(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (viewModel is null)
            return;
        viewModel.IsDragging = false;
        viewModel.OnPropertyChanged(nameof(viewModel.VITS2Noise1));
    }

    private void OnChangedNoise2(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (viewModel is null)
            return;
        viewModel.IsDragging = false;
        viewModel.OnPropertyChanged(nameof(viewModel.VITS2Noise2));
    }

    private void ClearVITS2Speed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            viewModel.ClrVITS2Speed();
        }
            
    }

    private void Tapped(object sender, TappedEventArgs e)
    {
        Log.Debug("Tapped");
        viewModel.IsDragging = true;
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