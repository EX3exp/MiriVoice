using Avalonia.Media;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mirivoice.ViewModels
{
    public class VoicersWindowViewModel: ViewModelBase
    {
        public ObservableCollection<VoicersVoicerButton> VoicersVoicerButtons { get; set; }
        public ObservableCollection<VoicersStyleBox> VoicersStyleBoxes { get; set; }
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                this.RaiseAndSetIfChanged(ref _name, value);
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _voice;
        public string Voice
        {
            get => _voice;
            set
            {
                this.RaiseAndSetIfChanged(ref _voice, value);
                OnPropertyChanged(nameof(Voice));
            }
        }

        private string _author;
        public string Author
        {
            get => _author;
            set
            {
                this.RaiseAndSetIfChanged(ref _author, value);
                OnPropertyChanged(nameof(Author));
            }
        }

        private string _engineer;
        public string Engineer
        {
            get => _engineer;
            set
            {
                this.RaiseAndSetIfChanged(ref _engineer, value);
                OnPropertyChanged(nameof(Engineer));
            }
        }

        private string _illustrator;
        public string Illustrator
        {
            get => _illustrator;
            set
            {
                this.RaiseAndSetIfChanged(ref _illustrator, value);
                OnPropertyChanged(nameof(Illustrator));
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                this.RaiseAndSetIfChanged(ref _description, value);
                OnPropertyChanged(nameof(Description));
            }
        }

        private string _web;
        public string Web
        {
            get => _web;
            set
            {
                this.RaiseAndSetIfChanged(ref _web, value);
                OnPropertyChanged(nameof(Web));
            }
        }

        private string _version;
        public string Version
        {
            get => _version;
            set
            {
                this.RaiseAndSetIfChanged(ref _version, value);
                OnPropertyChanged(nameof(Version));
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

        private ImageBrush _portrait;

        public ImageBrush Portrait
        {
            get => _portrait;

            set
            {
                this.RaiseAndSetIfChanged(ref _portrait, value);
                OnPropertyChanged(nameof(Portrait));
            }
        }


        private string _readme;
        public string Readme
        {
            get => _readme;
            set
            {
                this.RaiseAndSetIfChanged(ref _readme, value);
                OnPropertyChanged(nameof(Readme));
            }
        }
        public VoicersWindowViewModel(MainViewModel v)
        {
            List<VoicersVoicerButton> voicersVoicerButtons = new List<VoicersVoicerButton>();
            
            foreach (var voicer in v.voicerSelector.Voicers)
            {
                
                voicersVoicerButtons.Add(new VoicersVoicerButton(voicer, this, v));
                
            }

            VoicersVoicerButtons = new ObservableCollection<VoicersVoicerButton>(voicersVoicerButtons);
        }

        
    }
}
