using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Mirivoice.Commands;
using Mirivoice.Engines;
using Mirivoice.Mirivoice.Core;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Views;
using ReactiveUI;
using Serilog;
using System;

namespace Mirivoice.ViewModels
{
    public class ExpressionEditViewModelVITS2: ViewModelBase
    {
        public StackPanel CurrentExpression { get; set; }
        private readonly LineBoxView l;


        public int lastSpeed;
        public int lastNoise1;
        public int lastNoise2;

        

        public int MaxSpeed { get; set; } = 100;
        public int MinSpeed { get; set; } = 0;

        public int MaxNoise1 { get; set; } = 100;
        public int MinNoise1 { get; set; } = 0;

        public int MaxNoise2 { get; set; } = 100;
        public int MinNoise2 { get; set; } = 0;



        public int _vits2Speed;
        public int VITS2Speed
        {
            get
            {
                return _vits2Speed;
            }
            set
            {

                this.RaiseAndSetIfChanged(ref _vits2Speed, value);

                OnPropertyChanged(nameof(VITS2Speed));

                l.Exp.VITS2Speed = ScaleValue(100 - value, 0.5f, 1.5f);
                l.IsCacheIsVaild = false;
                

            }
        }

        public int _vits2Noise1;
        public int VITS2Noise1
        {
            get => _vits2Noise1;
            set
            {
                this.RaiseAndSetIfChanged(ref _vits2Noise1, value);
                OnPropertyChanged(nameof(VITS2Noise1));
                l.Exp.VITS2Noise1 = ScaleValue(value, -0.3335f, 1.6675f);
                l.IsCacheIsVaild = false;


            }
        }

        public int _vits2Noise2;
        public int VITS2Noise2
        {
            get => _vits2Noise2;
            set
            {
                
                this.RaiseAndSetIfChanged(ref _vits2Noise2, value);
                OnPropertyChanged(nameof(VITS2Noise2));
                l.Exp.VITS2Noise2 = ScaleValue(value, -0.4f, 2f);
                l.IsCacheIsVaild = false;


            }
        }
        public ExpressionEditViewModelVITS2(LineBoxView l)
        {
            this.l = l;

            _vits2Speed = 100 - GetSliderValue(l.Exp.VITS2Speed, 0.5f, 1.5f);
            _vits2Noise1 = GetSliderValue(l.Exp.VITS2Noise1, -0.3335f, 1.6675f);
            _vits2Noise2 = GetSliderValue(l.Exp.VITS2Noise2, -0.4f, 2f);

        }


        public static float ScaleValue(int input, float minValue, float maxValue)
        {

            return minValue + input / 100f * (maxValue - minValue);

        }

        public static int GetSliderValue(float value, float minValue, float maxValue)
        {
            return (int)((value - minValue) / (maxValue - minValue) * 100f);
        }
        public void ClrVITS2Speed()
        {
            VITS2Speed = 50;
        }

        public void ClrVITS2Noise1()
        {
            VITS2Noise1 = 50;
        }

        public void ClrVITS2Noise2()
        {
            VITS2Noise2 = 50;
        }
    }
}
