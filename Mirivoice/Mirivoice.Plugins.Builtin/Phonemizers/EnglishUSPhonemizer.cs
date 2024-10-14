using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Mirivoice.Plugins.Builtin.IPAConverters;
using Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers.Utils;
using System.Collections.Generic;
using System.Text;

namespace Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers
{
    public class EnglishUSPhonemizer : BasePhonemizer
    {
        public override BaseIPAConverter IPAConverter { get; set; } = new EnglishUSIPAConverter();
        public override bool UseWordDivider { get; set; } = false;

        protected override string[] SplitToWords(string sentence)
        {
            List<string> words = new List<string>();
            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (string word in sentence.Split())
            {
                char[] charArr = word.ToCharArray();
                bool LastWasPunctuation = false;
                foreach (char c in charArr)
                {
                    if (IsPunctuation(c.ToString()))
                    {
                        words.Add(sb.ToString());
                        sb.Clear();

                        sb.Append(c);
                        LastWasPunctuation = true;
                        continue;
                    }
                    if (!IsPunctuation(c.ToString()))
                    {
                        if (LastWasPunctuation)
                        {
                            words.Add(sb.ToString());
                            sb.Clear();
                            LastWasPunctuation = false;
                        }

                        sb.Append(c);
                    }

                }
                
                words.Add(sb.ToString());
                sb.Clear();
                
                ++index;
                if (index != sentence.Split().Length ) { 
                    words.Add(" ");
                }
            }
            return words.ToArray();
        }

        protected override string ToPhoneme(string word, out bool isEditable, out ProsodyType prosodyType)
        {
            if (word.Trim() == string.Empty)
            {

                isEditable = false;
                prosodyType = ProsodyType.None;
                return word;
            }
            isEditable = true;
            prosodyType = ProsodyType.Undefined;
            return EnglishUSPhonemizerUtil.WordToArpabet(word); // k ae t 
        }
    }
}
