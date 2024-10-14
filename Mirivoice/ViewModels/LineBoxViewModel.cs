using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Mirivoice.Mirivoice.Core;
using Mirivoice.Mirivoice.Core.Editor;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers;
using Mirivoice.Views;
using ReactiveUI;
using Serilog;
using System;
using System.Globalization;
using System.IO;


namespace Mirivoice.ViewModels
{
    public class LineBoxViewModel : VoicerSelectingViewModelBase
    {
        public override VoicerSelector voicerSelector { get; set; }
        public override MTextBoxEditor mTextBoxEditor { get; set; }
        private string _lineNo;
        public string LineNo
        {
            get => _lineNo;
            set
            {
                this.RaiseAndSetIfChanged(ref _lineNo, value);
                OnPropertyChanged(nameof(LineNo));
            }
        }

        private string _langCode;  
        public string LangCode
        {
            get => _langCode;
            set
            {
                this.RaiseAndSetIfChanged(ref _langCode, value);
                OnPropertyChanged(nameof(LangCode));
            }
        }
        public readonly Thickness OriginalThickness = new Thickness(1.0, 1.0, 1.0, 1.0);

        private Thickness _borderThickness;
        public Thickness BorderThickness
        {
            get => _borderThickness;
            set
            {
                this.RaiseAndSetIfChanged(ref _borderThickness, value);
                OnPropertyChanged(nameof(BorderThickness));
            }
        }

        string lineTextBeforeChangedCulture;

        public readonly IBrush OriginalBorderColor = new SolidColorBrush(Colors.Gray);

        public readonly Thickness OriginalMargin = new Thickness(0.0, 0.0, 0.0, 0.0);

        private Thickness _margin;
        public Thickness Margin
        {
            get => _margin;
            set
            {
                this.RaiseAndSetIfChanged(ref _margin, value);
                OnPropertyChanged(nameof(Margin));
            }
        }

        private IBrush _borderColor;
        public IBrush BorderColor
        {
            get => _borderColor;
            set
            {
                this.RaiseAndSetIfChanged(ref _borderColor, value);
                OnPropertyChanged(nameof(BorderColor));
            }
        }

        private string _lineText;
        public string LineText
        {
            get => _lineText;
            set
            {
                this.RaiseAndSetIfChanged(ref _lineText, value);
                OnPropertyChanged(nameof(LineText));
                
            }
        }

        private string _voicerType;
        public string VoicerType
        {
            get => _voicerType;
            set
            {
                this.RaiseAndSetIfChanged(ref _voicerType, value);
                OnPropertyChanged(nameof(VoicerType));
            }
        }
        private bool _isSelected = false;

        public bool IsSelected
        {
            get => _isSelected;
            set {
                this.RaiseAndSetIfChanged(ref _isSelected, value);
                OnPropertyChanged(nameof(IsSelected));
            }
        }


        private bool _canHitTest = true;
        public bool CanHitTest
        {
            get => _canHitTest;
            set
            {
                this.RaiseAndSetIfChanged(ref _canHitTest, value);
                OnPropertyChanged(nameof(CanHitTest));
            }
        }

        private ImageBrush _icon;

        public ImageBrush Icon
        {
            get => _icon;

            set 
            {
                this.RaiseAndSetIfChanged(ref _icon, value);
                OnPropertyChanged(nameof(Icon));
            }
        }

        public BasePhonemizer phonemizer;

        public override void OnVoicerChanged(Voicer voicer)
        {
            //Log.Debug($"OnVoicerChanged: {voicer.NickAndStyle}");
            VoicerInfo vInfo = voicer.Info;
            
            if (phonemizer is null)
            {
                phonemizer = GetPhonemizer(voicerSelector.CurrentVoicer.Info.LangCode);
            }
            
            l.IsCacheIsVaild = false;

            LangCode = voicer.Info.LangCode.ToUpper().Substring(0, 2);
            VoicerType = voicer.Info.Type;
            if (vInfo.Icon != null && vInfo.Icon != string.Empty)
            {
                string voicerIconPath = Path.Combine(voicer.RootPath,
                vInfo.Icon.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(voicerIconPath))
                {
                    return;
                }

                Icon
                = new ImageBrush(new Bitmap(voicerIconPath))
                {
                    Stretch = Stretch.UniformToFill,
                };

            }
            else
            {
                var uri = new Uri("avares://Mirivoice.Main/Assets/default_icon.bmp");
                var assets = AssetLoader.Open(uri);

                using (var stream = assets)
                {
                    var bitmap = new Bitmap(stream);
                    Icon = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                }
            }
        }

        bool cultureChangedFirst = false;
        public override void OnVoicerCultureChanged(CultureInfo culture)
        {
            phonemizer = GetPhonemizer(voicerSelector.CurrentVoicer.Info.LangCode); // get phonemizer
            if (lineTextBeforeChangedCulture is not null && LineText is not null &&
                LineText.Equals(lineTextBeforeChangedCulture))
            {
                if (cultureChangedFirst)
                {
                    l.ShowBackUp = true; // restore backup MResults
                    cultureChangedFirst = false;
                }
                
                
            }
            else
            {
                lineTextBeforeChangedCulture = LineText;
                cultureChangedFirst = true;
                l.ShowBackUp = false; // backup current MResults
            }
            
        }

        private readonly LineBoxView l;
        public LineBoxViewModel(LineBoxView l): base(true)
        {
            this.l = l;
            int DefaultVoicerIndex = MainManager.Instance.DefaultVoicerIndex;
            int DefaultMetaIndex = MainManager.Instance.DefaultMetaIndex;
            voicerSelector.CurrentDefaultVoicerIndex = DefaultVoicerIndex;
            voicerSelector.Voicers[DefaultVoicerIndex].CurrentVoicerMeta = voicerSelector.Voicers[DefaultVoicerIndex].VoicerMetaCollection[DefaultMetaIndex];
            _borderThickness = OriginalThickness;
            _borderColor = OriginalBorderColor;
            Margin = OriginalMargin;
            VoicerInfo vInfo = voicerSelector.CurrentVoicer.Info;
            Voicer voicer = voicerSelector.CurrentVoicer;
            lineTextBeforeChangedCulture = LineText;
            if (vInfo.Icon != null && vInfo.Icon != string.Empty)
            {
                string voicerIconPath = Path.Combine(voicer.RootPath,
                vInfo.Icon.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(voicerIconPath))
                {
                    return;
                }

                Icon
                = new ImageBrush(new Bitmap(voicerIconPath))
                {
                    Stretch = Stretch.UniformToFill,
                };

            }
            else
            {
                var uri = new Uri("avares://Mirivoice.Main/Assets/default_icon.bmp");
                var assets = AssetLoader.Open(uri);

                using (var stream = assets)
                {
                    var bitmap = new Bitmap(stream);
                    Icon = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                }
            }

        }

        public LineBoxViewModel(int voicerIndex, int metaIndex, LineBoxView l) : base(voicerIndex, true)
        {
            this.l = l;
            voicerSelector.Voicers[voicerIndex].CurrentVoicerMeta = voicerSelector.Voicers[voicerIndex].VoicerMetaCollection[metaIndex];
            OnVoicerChanged(voicerSelector.CurrentVoicer);
            _borderThickness = OriginalThickness;
            _borderColor = OriginalBorderColor;
            Margin = OriginalMargin;
            VoicerInfo vInfo = voicerSelector.CurrentVoicer.Info;
            Voicer voicer = voicerSelector.CurrentVoicer;
            if (vInfo.Icon != null && vInfo.Icon != string.Empty)
            {
                string voicerIconPath = Path.Combine(voicer.RootPath,
                vInfo.Icon.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(voicerIconPath))
                {
                    return;
                }

                Icon
                = new ImageBrush(new Bitmap(voicerIconPath))
                {
                    Stretch = Stretch.UniformToFill,
                };

            }
            else
            {
                var uri = new Uri("avares://Mirivoice.Main/Assets/default_icon.bmp");
                var assets = AssetLoader.Open(uri);

                using (var stream = assets)
                {
                    var bitmap = new Bitmap(stream);
                    Icon = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                }
            }

        }

        public void SetLineNo(int lineNo)
        {
            LineNo = lineNo.ToString();
        }

        public static BasePhonemizer GetPhonemizer(string langCode)
        {
            if (langCode is null)
            {
                throw new ArgumentNullException("langCode");
            }
            CultureInfo culture = new CultureInfo(langCode);

            Log.Information("Culture: {0}", culture.ThreeLetterWindowsLanguageName);
            switch (culture.ThreeLetterWindowsLanguageName)
            {
                case "KOR":
                    return new KoreanPhonemizer();
                case "ENU":
                    return new EnglishUSPhonemizer(); 
                default:
                    return new DefaultPhonemizer();
            }
        }
    }


}
