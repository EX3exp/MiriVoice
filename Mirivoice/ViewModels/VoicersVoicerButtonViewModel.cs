using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Mirivoice.Engines;
using Mirivoice.Mirivoice.Core.Format;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Mirivoice.ViewModels
{
    public class VoicersVoicerButtonViewModel: ViewModelBase
    {
        private readonly VoicersWindowViewModel v;
        private readonly MainViewModel mv;
        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSelected, value);
                OnPropertyChanged(nameof(IsSelected));
            }
        }
        public Voicer Voicer { get; set; }
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
        public VoicersVoicerButtonViewModel(Voicer voicer, VoicersWindowViewModel v, MainViewModel mv)
        {
            Voicer = voicer;
            this.v = v;
            VoicerInfo vInfo = Voicer.Info;
            this.mv = mv;
            LangCode = vInfo.LangCode.ToUpper().Substring(0, 2);

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

        public void OnButtonClick()
        {
            foreach (var button in v.VoicersVoicerButtons)
            {
                button.viewModel.IsSelected = false;
            }
            IsSelected = true;

            List<VoicersStyleBox> voicersStyleBoxes = new List<VoicersStyleBox>();
            int i = 0;
            VoicerMeta[] voicerMetas = Voicer.VoicerMetaCollection.ToArray();
            foreach (var meta in voicerMetas )
            {
                Log.Debug($"VoicerMeta {i}: {meta.Style}");
                voicersStyleBoxes.Add(new VoicersStyleBox(Voicer, i, mv));
                ++i;
            }
            v.VoicersStyleBoxes = new ObservableCollection<VoicersStyleBox>(voicersStyleBoxes);
            v.OnPropertyChanged(nameof(v.VoicersStyleBoxes));
            v.Name = Voicer.Info.Name;
            v.Voice = Voicer.Info.Voice;
            v.Author = Voicer.Info.Author;
            v.Engineer = Voicer.Info.Engineer;
            v.Illustrator = Voicer.Info.Illustrator;
            v.Version = Voicer.Info.Version;
            v.Description = Voicer.Info.Description;
            v.Web = Voicer.Info.Web;    

            VoicerInfo vInfo = Voicer.Info;
            Voicer voicer = Voicer;
            if (vInfo.Portrait != null && vInfo.Portrait != string.Empty)
            {
                string voicerPortraitPath = Path.Combine(voicer.RootPath,
                vInfo.Portrait.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(voicerPortraitPath))
                {
                    return;
                }

                v.Portrait
                = new ImageBrush(new Bitmap(voicerPortraitPath))
                {
                    Stretch = Stretch.UniformToFill,
                };

            }
            else
            {
                var uri = new Uri("avares://Mirivoice.Main/Assets/default_portrait.bmp");
                var assets = AssetLoader.Open(uri);

                using (var stream = assets)
                {
                    var bitmap = new Bitmap(stream);
                    v.Portrait = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                }
            }

            if (vInfo.Icon != null && vInfo.Icon != string.Empty)
            {
                string voicerIconPath = Path.Combine(voicer.RootPath,
                vInfo.Icon.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(voicerIconPath))
                {
                    return;
                }

                v.Icon
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
                    v.Icon = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                }
            }

            if (vInfo.Readme != null && vInfo.Readme != string.Empty)
            {
                string readmePath = Path.Combine(voicer.RootPath, Voicer.Info.Readme);
                if (File.Exists(readmePath))
                {
                    v.Readme = File.ReadAllText(readmePath);
                }
                else
                {
                    v.Readme = string.Empty;
                }
            }
            else
            {
                v.Readme = string.Empty;
            }


        }
    }
}
