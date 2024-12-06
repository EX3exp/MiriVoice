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
using System.Windows.Input;
using Mirivoice.Commands;

namespace Mirivoice;

public partial class ExpressionEditViewVITS2 : UserControl
{
    public ExpressionEditViewModelVITS2 viewModel;
    VoicerMetaType vMeta;

    public ExpressionEditViewVITS2(LineBoxView l)
    {
        this.vMeta = vMeta;
        InitializeComponent(l);
        this.FindControl<Slider>("SpeedSlider").AddHandler(PointerPressedEvent, OnPointerPressedSpeed, handledEventsToo: true);
        this.FindControl<Slider>("SpeedSlider").AddHandler(PointerReleasedEvent, OnPointerReleasedSpeed, handledEventsToo: true);

        this.FindControl<Slider>("Noise1Slider").AddHandler(PointerPressedEvent, OnPointerPressedNoise1, handledEventsToo: true);
        this.FindControl<Slider>("Noise1Slider").AddHandler(PointerReleasedEvent, OnPointerReleasedNoise1, handledEventsToo: true);

        this.FindControl<Slider>("Noise2Slider").AddHandler(PointerPressedEvent, OnPointerPressedNoise2, handledEventsToo: true);
        this.FindControl<Slider>("Noise2Slider").AddHandler(PointerReleasedEvent, OnPointerReleasedNoise2, handledEventsToo: true);

        this.FindControl<NumericUpDown>("NumericSpeed").AddHandler(NumericUpDown.ValueChangedEvent, OnValueChangedSpeed, handledEventsToo: true);
        this.FindControl<NumericUpDown>("NumericNoise1").AddHandler(NumericUpDown.ValueChangedEvent, OnValueChangedNoise1, handledEventsToo: true);
        this.FindControl<NumericUpDown>("NumericNoise2").AddHandler(NumericUpDown.ValueChangedEvent, OnValueChangedNoise2, handledEventsToo: true);
    }

    bool IsDraggingSpeed = false;
    bool IsDraggingNoise1 = false;
    bool IsDraggingNoise2 = false;

    VITS2SetExpCommand vITS2SetExpCommandSpeed;
    VITS2SetExpCommand vITS2SetExpCommandNoise1;
    VITS2SetExpCommand vITS2SetExpCommandNoise2;

    private void OnPointerPressedSpeed(object? sender, PointerPressedEventArgs e)
    {

        IsDraggingSpeed = true;
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            viewModel.ClrVITS2Speed();
        }
        vITS2SetExpCommandSpeed = new VITS2SetExpCommand(viewModel, ExpVITS2.Speed);
    }

    private void OnPointerPressedNoise1(object? sender, PointerPressedEventArgs e)
    {

        IsDraggingNoise1 = true;
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            viewModel.ClrVITS2Noise1();
        }

        vITS2SetExpCommandNoise1 = new VITS2SetExpCommand(viewModel, ExpVITS2.Noise1);
    }

    private void OnPointerPressedNoise2(object? sender, PointerPressedEventArgs e)
    {

        IsDraggingNoise2 = true;

        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            viewModel.ClrVITS2Noise2();
        }

        vITS2SetExpCommandNoise2 = new VITS2SetExpCommand(viewModel, ExpVITS2.Noise2);
    }

    private void OnPointerReleasedSpeed(object? sender, PointerReleasedEventArgs e)
    {
        

        if (!MainManager.Instance.cmd.IsAlreadyExecuted(vITS2SetExpCommandSpeed))
            MainManager.Instance.cmd.ExecuteCommand(vITS2SetExpCommandSpeed);

        IsDraggingSpeed = false;
    }

    private void OnPointerReleasedNoise1(object? sender, PointerReleasedEventArgs e)
    {
        if (!MainManager.Instance.cmd.IsAlreadyExecuted(vITS2SetExpCommandNoise1))
            MainManager.Instance.cmd.ExecuteCommand(vITS2SetExpCommandNoise1);
        IsDraggingNoise1 = false;
    }

    private void OnPointerReleasedNoise2(object? sender, PointerReleasedEventArgs e)
    {
        if (!MainManager.Instance.cmd.IsAlreadyExecuted(vITS2SetExpCommandNoise2))
            MainManager.Instance.cmd.ExecuteCommand(vITS2SetExpCommandNoise2);
        IsDraggingNoise2 = false;
    }

    bool numericChangedSpeed = false;
    bool numericChangedNoise1 = false;
    bool numericChangedNoise2 = false;

    private void OnValueChangedSpeed(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        
        numericChangedSpeed = true;
    }

    private void OnValueChangedNoise1(object? sender, NumericUpDownValueChangedEventArgs e)
    {

        numericChangedNoise1 = true;
    }

    private void OnValueChangedNoise2(object? sender, NumericUpDownValueChangedEventArgs e)
    {

        numericChangedNoise2 = true;
    }

    private void OnChangedSpeed(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (viewModel is null)
            return;


        if (!IsDraggingSpeed || numericChangedSpeed)
        {
            
            if (!MainManager.Instance.cmd.IsAlreadyExecuted(vITS2SetExpCommandSpeed))
            {
                vITS2SetExpCommandSpeed = new VITS2SetExpCommand(viewModel, ExpVITS2.Speed);
                MainManager.Instance.cmd.ExecuteCommand(vITS2SetExpCommandSpeed);
            }
            numericChangedSpeed = false;

        }

        viewModel.OnPropertyChanged(nameof(viewModel.VITS2Speed));
    }

    
    private void OnChangedNoise1(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (viewModel is null)
            return;


        if (!IsDraggingNoise1 || numericChangedNoise1)
        {
            
            if (!MainManager.Instance.cmd.IsAlreadyExecuted(vITS2SetExpCommandNoise1))
            {
                vITS2SetExpCommandNoise1 = new VITS2SetExpCommand(viewModel, ExpVITS2.Noise1);
                MainManager.Instance.cmd.ExecuteCommand(vITS2SetExpCommandNoise1);
            }
            numericChangedNoise1 = false;
        }
        viewModel.OnPropertyChanged(nameof(viewModel.VITS2Noise1));
    }

    private void OnChangedNoise2(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (viewModel is null)
            return;


        if (!IsDraggingNoise2 || numericChangedNoise2)
        {
            
            if (!MainManager.Instance.cmd.IsAlreadyExecuted(vITS2SetExpCommandNoise2))
            {
                vITS2SetExpCommandNoise2 = new VITS2SetExpCommand(viewModel, ExpVITS2.Noise2);
                MainManager.Instance.cmd.ExecuteCommand(vITS2SetExpCommandNoise2);

            }
            numericChangedNoise2 = false;
        }
        viewModel.OnPropertyChanged(nameof(viewModel.VITS2Noise2));
    }


    private void InitializeComponent(LineBoxView l)
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = viewModel = new ExpressionEditViewModelVITS2(l);
    }

    
}