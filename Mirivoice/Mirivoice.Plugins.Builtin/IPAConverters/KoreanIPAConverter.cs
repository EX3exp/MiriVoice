using Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers.Utils;
using System.Collections.Generic;

namespace Mirivoice.Mirivoice.Plugins.Builtin.IPAConverters
{
    public class KoreanIPAConverter : BaseIPAConverter
    {
        // https://en.wikipedia.org/wiki/Help:IPA/Korean
        // Note : 
        // ㄱ, ㄷ, ㅂ sounds are aspirated in the beginning of a word.
        // If ㅇ is at beginning of a word, it is silent.


        readonly Dictionary<string, string> FirstConsonant2IPA 
            = new Dictionary<string, string>()
            {
                {"ㄱ", "g"},
                {"-ㄱ", "k" },
                {"ㄴ", "n" },
                {"ㄷ", "d" },
                {"-ㄷ", "t" },
                {"ㄹ", "ɾ" },
                {"ㅁ", "m" },
                {"ㅂ", "b" },
                {"-ㅂ", "p" },
                {"ㅅ", "s" },
                {"ㅇ", "" }, 
                {"ㅈ", "dʑ" },
                {"ㅊ", "tɕh" },
                {"ㅋ", "kʰ" },
                {"ㅌ", "tʰ" },
                {"ㅍ", "pʰ" },
                {"ㅎ", "h" },
                {"ㄲ", "k͈" },
                {"ㄸ", "t͈" },
                {"ㅃ", "p͈" },
                {"ㅆ", "s͈" },
                {"ㅉ", "ts͈" }
            };

        readonly Dictionary<string, string> Vowel2IPA
            = new Dictionary<string, string>()
            {
                {"ㅏ", "a" },
                {"ㅐ", "ɛ" },
                {"ㅑ", "ja" },
                {"ㅒ", "jɛ" },
                {"ㅓ", "ʌ" },
                {"ㅔ", "ɛ" },
                {"ㅕ", "jʌ" },
                {"ㅖ", "jɛ" },
                {"ㅗ", "o" },
                {"ㅘ", "wa" },
                {"ㅙ", "wɛ" },
                {"ㅚ", "wɛ" },
                {"ㅛ", "jo" },
                {"ㅜ", "u" },
                {"ㅝ", "wʌ" },
                {"ㅞ", "wɛ" },
                {"ㅟ", "wi" },
                {"ㅠ", "ju" },
                {"ㅡ", "ɯ" },
                {"ㅢ", "ɰi" },
                {"ㅣ", "i" }
            };

        readonly Dictionary<string, string> LastConsonant2IPA
            = new Dictionary<string, string>()
            {
                {"ㄱ", "k̚" },
                {"ㄴ", "n" },
                {"ㄷ",  "t̚"},
                {"ㄹ", "ɭ" },
                {"ㅁ", "m" },
                {"ㅂ", "p̚" },
                {"ㅇ", "ŋ" },
                {" ",string.Empty }
            };
        public override string ConvertToIPA(string phoneme, bool isFirstPhoneme)
        {
            
            string result = phoneme;
            if (KoreanPhonemizerUtil.IsHangeul(phoneme))
            {

                string phoneme_ = KoreanPhonemizerUtil.Variate(null, phoneme, null);
                var splited = KoreanPhonemizerUtil.Separate(phoneme_);
                string firstConsonant = (string)splited[0];

                if ( isFirstPhoneme && (firstConsonant == "ㄱ" || firstConsonant == "ㄷ" || firstConsonant == "ㅂ"))
                {
                   firstConsonant = $"-{firstConsonant}";
                }
                string _result = $"{FirstConsonant2IPA[firstConsonant]}{Vowel2IPA[(string)splited[1]]}{LastConsonant2IPA[(string)splited[2]]}";
                result = string.Join("\t", _result.ToCharArray());

                //Log.Information("ConvertToIPA: {phoneme} -> {result}", phoneme, result);
            }

            return result;
        }
    }
}
