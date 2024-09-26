using Mirivoice.Mirivoice.Core.Editor;
using Mirivoice.ViewModels;
using System.Globalization;
using VYaml.Annotations;

namespace Mirivoice.Mirivoice.Core.Format
{

    

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

        public override MTextBoxEditor mTextBoxEditor { get; set; }


        public bool IsEditable { get; set; } = false;
        public MResult(string word, string phoneme, bool isEditable): base(phoneme, false)
        {
            this.Word = word;
            this.IsEditable = isEditable;
        }

        public MResult(MResultPrototype mResultPrototype): base(mResultPrototype.Phoneme, false)
        {
            this.Word = mResultPrototype.Word;
            this.IsEditable = mResultPrototype.IsEditable;

        }
        public override void OnVoicerChanged(Voicer value) { }
        public override void OnVoicerCultureChanged(CultureInfo culture) { }
    }
}
