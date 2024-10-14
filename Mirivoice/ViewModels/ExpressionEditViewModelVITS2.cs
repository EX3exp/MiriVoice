using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Mirivoice.Commands;
using Mirivoice.Engines;
using Mirivoice.Mirivoice.Core;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Views;
using ReactiveUI;
using System;

namespace Mirivoice.ViewModels
{
    public class ExpressionEditViewModelVITS2: ViewModelBase
    {
        public StackPanel CurrentExpression { get; set; }
        private readonly LineBoxView l;

        public bool NotProcessingSpeedCommand = false;
        public bool NotProcessingNoise1Command = false;
        public bool NotProcessingNoise2Command = false;

        public int lastSpeed;
        public int lastNoise1;
        public int lastNoise2;

        public bool UndobackupSpeed = false;
        public bool UndobackupNoise1 = false;
        public bool UndobackupNoise2 = false;

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

                lastSpeed = _vits2Speed;

                if (!IsDragging)
                {
                    if (NotProcessingSpeedCommand)
                    {
                        if (!UndobackupSpeed)
                        {


                            SetSpeedCommand.Backup(lastSpeed);
                            UndobackupSpeed = true;
                        }
                        MainManager.Instance.cmd.ExecuteCommand(SetSpeedCommand);

                        UndobackupSpeed = false;
                    }
                    else
                    {
                        NotProcessingSpeedCommand = false;

                    }
                }
                else
                {
                    NotProcessingSpeedCommand = true;
                }

                
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
                lastNoise1 = _vits2Noise1;

                if (!IsDragging)
                {
                    if (NotProcessingNoise1Command)
                    {
                        if (!UndobackupNoise1)
                        {
                            SetNoise1Command.Backup(lastNoise1);
                            UndobackupNoise1 = true;
                        }
                        MainManager.Instance.cmd.ExecuteCommand(SetNoise1Command);
                        UndobackupNoise1 = false;
                    }
                    else
                    {
                        NotProcessingNoise1Command = false;

                    }
                }
                else
                {
                    NotProcessingNoise1Command = true;
                }
                

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
                lastNoise2 = _vits2Noise2;

                if (!IsDragging)
                {
                    if (NotProcessingNoise2Command)
                    {
                        if (!UndobackupNoise2)
                        {
                            SetNoise2Command.Backup(lastNoise2);
                            UndobackupNoise2 = true;
                        }
                        MainManager.Instance.cmd.ExecuteCommand(SetNoise2Command);
                        UndobackupNoise2 = false;
                    }
                    else
                    {
                        NotProcessingNoise2Command = false;

                    }
                }
                else
                {
                    NotProcessingNoise2Command = true;
                }
                

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
            SetCommands();
        }

        public bool IsDragging = false;

        


        MOriginator<int> SetSpeedOriginator;
        public MementoCommand<int> SetSpeedCommand;
        MOriginator<int> SetNoise1Originator;
        public MementoCommand<int> SetNoise1Command;
        MOriginator<int> SetNoise2Originator;
        public MementoCommand<int> SetNoise2Command;
        void SetCommands()
        {
            SetSpeedOriginator = new VITS2SetExpOriginator(ref _vits2Speed, this, ExpVITS2.Speed);
            SetSpeedCommand = new MementoCommand<int>(SetSpeedOriginator);
            NotProcessingSpeedCommand = true;

            SetNoise1Originator = new VITS2SetExpOriginator(ref _vits2Noise1, this, ExpVITS2.Noise1);
            SetNoise1Command = new MementoCommand<int>(SetNoise1Originator);
            NotProcessingNoise1Command = true;

            SetNoise2Originator = new VITS2SetExpOriginator(ref _vits2Noise2, this, ExpVITS2.Noise2);
            SetNoise2Command = new MementoCommand<int>(SetNoise2Originator);
            NotProcessingNoise2Command = true;
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
