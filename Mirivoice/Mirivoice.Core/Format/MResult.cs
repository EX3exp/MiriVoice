using Avalonia.Media;
using Mirivoice.Commands;
using Mirivoice.Mirivoice.Core.Editor;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using ReactiveUI;
using Serilog;
using System.Drawing;
using System.Globalization;
using VYaml.Annotations;

namespace Mirivoice.Mirivoice.Core.Format
{


    public enum ProsodyType
    {
        Undefined = -1,
        Bos = 0,
        None = 1,
        Space = 2,
        Eos = 3
    }
    public class MResult : VoicerSelectingViewModelBase
    {
        // each phoneme blocks will own One Syllable only 

        // Sentence
        // --> (Spliter) --> Word
        // --> (Cleaner) --> Grapheme
        // --> (Phonemizer) --> Phonemes
        // --> (IPAConverters) --> TTSPhonemes

        // Grapheme is feeded to phonemizer.
        // could contain puncuations, speaker-specific notations, etc.
        //public string Grapheme { get; private set; } // [cat] -- [猫] -- 집, [고], 양, 이 

        // English: ARPAbet notation
        // Japanese: Hiragana
        // Korean: Hangul
        //public string[] Phonemes { get; set; } // [kaet]　-- [ね], [こ] -- 집, [꼬], 양, 이

        // Phoneme that actually feeded to tts --  usually IPA
        // It differs by TTS engine.
        //public string[] TTSPhonemes { get; set; } // [kæt] -- [ne], [.ko] -- ʨip̚, [k͈o], jaŋ, .i
        public string Word { get; set; }

        public int _currentProsodyIndex;
        public bool NotProcessingSetProsodyCommand = false;
        int lastProsodyIndex;
        public override MTextBoxEditor mTextBoxEditor { get; set; }

        public override void OnStyleChanged() { }
        private ImageBrush _bosIcon;
        public ImageBrush BosIcon
        {
            get => _bosIcon;
            set
            {
                this.RaiseAndSetIfChanged(ref _bosIcon, value);
                OnPropertyChanged(nameof(BosIcon));
            }

        }
        private ImageBrush _eosIcon;
        public ImageBrush EosIcon
        {
            get => _eosIcon;
            set
            {
                this.RaiseAndSetIfChanged(ref _eosIcon, value);
                OnPropertyChanged(nameof(EosIcon));
            }
        }

        private ImageBrush _nonIcon;
        public ImageBrush NonIcon
        {
            get => _nonIcon;
            set
            {
                this.RaiseAndSetIfChanged(ref _nonIcon, value);
                OnPropertyChanged(nameof(NonIcon));
            }
        }

        private ImageBrush _spaceIcon;
        public ImageBrush SpaceIcon
        {
            get => _spaceIcon;
            set
            {
                this.RaiseAndSetIfChanged(ref _spaceIcon, value);
                OnPropertyChanged(nameof(SpaceIcon));
            }
        }

        private int _prosody;

        bool UndobackupedProsody = false;

        public int Prosody
        {
            get
            {
                return _prosody;
            }
            set
            {
                if (value == -1)
                {
                    return;
                }
                lastProsodyIndex = _prosody;

                

                if (NotProcessingSetProsodyCommand)
                {
                    if (!UndobackupedProsody)
                    {


                        setProsodyCommand.Backup(lastProsodyIndex);
                        UndobackupedProsody = true;
                    }
                    MainManager.Instance.cmd.ExecuteCommand(setProsodyCommand);

                    UndobackupedProsody = false;
                }
                else
                {
                    NotProcessingSetProsodyCommand = false;

                }
                this.RaiseAndSetIfChanged(ref _prosody, value);
                
                OnPropertyChanged(nameof(Prosody));

                if (l is not null)
                {
                    l.IsCacheIsVaild = false;

                }
            }
        }

        public bool IsEditable { get; set; } = false;

        private readonly LineBoxView l;
        public MResult(string word, string phoneme, bool isEditable, ProsodyType prosodyType, LineBoxView l=null): base(phoneme, false)
        {
            this.Word = word;
            this.IsEditable = isEditable;
            Prosody = (int)prosodyType;
            SetIcons();
            this.l = l;
            SetCommands();
        }

        public MResult(MResultPrototype mResultPrototype, LineBoxView l) : base(mResultPrototype.Phoneme, false)
        {
            this.Word = mResultPrototype.Word;
            this.IsEditable = mResultPrototype.IsEditable;
            this.Prosody = mResultPrototype.ProsodyType;
            SetIcons();
            this.l = l;
            SetCommands();
        }

        void SetIcons()
        {
            BosIcon = MainManager.Instance.IconM.BosIcon;
            EosIcon = MainManager.Instance.IconM.EosIcon;
            NonIcon = MainManager.Instance.IconM.NonIcon;
            SpaceIcon = MainManager.Instance.IconM.SpaceIcon;
        }

        MOriginator<int> setProsodyOriginator;
        MementoCommand<int> setProsodyCommand;
        void SetCommands()
        {
            setProsodyOriginator = new SetProsodyOriginator(ref _prosody, this);
            setProsodyCommand = new MementoCommand<int>(setProsodyOriginator);
            NotProcessingSetProsodyCommand = true;
        }
        public override void OnVoicerChanged(Voicer value) { }
        public override void OnVoicerCultureChanged(CultureInfo culture) { }

    }
}
