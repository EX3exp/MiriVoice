using Mirivoice.Mirivoice.Plugins.Builtin.IPAConverters;
using Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers
{
    public class KoreanPhonemizer: BasePhonemizer
    {
        public override BaseIPAConverter IPAConverter { get; set; } = new KoreanIPAConverter();
        public override bool UseWordDivider { get; set; } = true;
        protected override string Variate(string sentence)
        {
            try
            {
                List<string> __words = new List<string>(); // not cleaned

                List<string> _words = new List<string>(); // cleaned

                List<string> words = new List<string>();

                // clean text. converts numbers into korean pronunciation
                bool doNotTideUpStringBuilder = false;
                bool prevWasNumber = false;
                bool prevWasBoundNoun = false;
                bool boundNounEnded = false; 

                StringBuilder sb = new StringBuilder();
                int index = 0;
                char[] _charArr = sentence.ToCharArray();
                 foreach (char c in _charArr)
                {
                    if (prevWasNumber && IsPunctuation(c.ToString())){
                        doNotTideUpStringBuilder = true;
                        prevWasNumber = false;
                    }
                    else if (IsNumber(c.ToString()))
                    {
                        // collect words containing numbers together 
                        // e.g ) 12345개

                        doNotTideUpStringBuilder = true;
                        prevWasNumber = true;

                    }
                    else if (_charArr.Length > index + 1 && KoreanNumberProcessor.IsPartOfBoundNoun(c, _charArr[index + 1]))
                    {
                        doNotTideUpStringBuilder = true;
                        prevWasBoundNoun = true;
                    }

                    else if (!IsNumber(c.ToString()) || prevWasBoundNoun)
                    {
                        doNotTideUpStringBuilder = false; // will tide up stringbuilder in this loop
                    }
                    else if (KoreanNumberProcessor.IsBoundNoun(c)) // 1 char bound noun
                    {
                        doNotTideUpStringBuilder = false; // will tide up stringbuilder in this loop
                        boundNounEnded = true;
                    }

                    else if (prevWasBoundNoun) // 2 char bound noun
                    {
                        prevWasBoundNoun = false; // maximum 2 char bound noun is supported only
                        doNotTideUpStringBuilder = false; 
                        boundNounEnded = true;
                    }
                    else
                    {
                        doNotTideUpStringBuilder = false;
                    }

                    if (boundNounEnded)
                    {
                        boundNounEnded = false;
                        sb.Append(c);
                        
                        __words.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (doNotTideUpStringBuilder && index < _charArr.Length - 1)
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        prevWasNumber = false;
                        sb.Append(c);
                        __words.Add(sb.ToString());
                        sb.Clear();
                    }
                    
                    ++index;
                }

                
                foreach (string word in __words)
                {
                    if (IsNumber(word.ToCharArray()[0].ToString()))
                    {
                        _words.Add(KoreanNumberProcessor.ConvertNum(word));
                    }
                    else
                    {
                        _words.Add(word);
                    }
                }

                char[] charArr = string.Join("", _words).ToCharArray();


                foreach (char c in charArr)
                {
                    words.Add(c.ToString());

                }

                List<string> phonemes = new List<string>();
                string prev = null;
                string next = null;
                bool nextIsBlank = false;

                if (words.Count > 1)
                {
                    if (words[1] == " " || IsPunctuation(words[1])) 
                        {
                        if (words.Count > 2)
                        {

                            next = words[2];
                        }
                        else
                        {
                            next = null;
                        }
                    }
                    else
                    {
                        next = words[1];
                    }
                    

                }

                


                for (int i = 0; i < charArr.Length; i++)
                {
                    if (words[i] == " " || IsPunctuation(words[i]))
                    {
                        phonemes.Add(words[i]);

                        continue;
                    }
                    else if  ( !KoreanPhonemizerUtil.IsHangeul(words[i]))
                    {
                        prev = null;
                        if (words.Count > i + 2 && KoreanPhonemizerUtil.IsHangeul(words[i + 2]))
                        {
                            if (words.Count > i + 3 && (words[i + 2] == " " || IsPunctuation(words[i + 2])))
                            {
                                prev = words[i + 1];
                                next = words[i + 3];
                            }
                            else
                            {
                                next = words[i + 2];
                            }
                              
                        }
                        else
                        {
                            next = null;
                        }
                        phonemes.Add(words[i]);
                        continue;
                    }

                    if (i > 1 && words.Count > i + 1 && (words[i - 1] == " " || IsPunctuation(words[i - 1])))
                    {
                        next = words[i + 1];
                    }
                    
                    phonemes.Add(KoreanPhonemizerUtil.Variate(prev, words[i], next));
                    prev = words[i];

                    if (words.Count > i + 2 && KoreanPhonemizerUtil.IsHangeul(words[i + 2]))
                    {
                        if (words.Count > i + 2 && (words[i + 2] == " " || IsPunctuation(words[i + 2])))
                        {
                            next = words[i + 3];
                        }
                        else
                        {
                            next = words[i + 2];
                        }

                    }
                    else
                    {
                        next = null;
                    }
                    //Log.Debug($"prev: {prev}, curr: {words[i]}, next: {next}");
                    continue;
                }
                //Log.Debug("Phonemes: {phonemes}", phonemes);
                
                return string.Join("", phonemes);
            }
            catch (Exception e)
            {
                Log.Error("Error in Variating: {e}", e);
                return sentence;
            }
        }
    }
}
