﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Mirivoice.Mirivoice.Core.Format;
using Avalonia.Media;
using Avalonia.Platform;
using Mirivoice.Mirivoice.Core.Editor;

using System.IO;
using Avalonia.Media.Imaging;

namespace Mirivoice.ViewModels
{
    public class VoicersVoicerButtonViewModel: ViewModelBase
    {
        private readonly VoicersWindowViewModel v;
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
        public VoicersVoicerButtonViewModel(Voicer voicer, VoicersWindowViewModel v)
        {
            Voicer = voicer;
            this.v = v;
            VoicerInfo vInfo = Voicer.Info;

            LangCode = vInfo.LangCode.ToUpper();

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
                var uri = new Uri("avares://Mirivoice/Assets/default_icon.bmp");
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

            v.Name = Voicer.Info.Name;
            v.Voice = Voicer.Info.Voice;
            v.Author = Voicer.Info.Author;
            v.Engineer = Voicer.Info.Engineer;
            v.Illustrator = Voicer.Info.Illustrator;
            v.Version = Voicer.Info.Version;
            v.Description = Voicer.Info.Description;

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
                var uri = new Uri("avares://Mirivoice/Assets/default_portrait.bmp");
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
                var uri = new Uri("avares://Mirivoice/Assets/default_icon.bmp");
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
