using Avalonia.Threading;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Mirivoice.Plugins.Builtin.IPAConverters;
using Mirivoice.Views;
using Serilog;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers
{
    public abstract class BasePhonemizer
    {
        List<string> IPAPhonemes = new List<string>();
        public virtual BaseIPAConverter IPAConverter { get; set; }  // should be overrided
        public virtual bool UseWordDivider { get; set; } = false; // if true, insert blank block between words
        private static List<string> endPuncs = new List<string> { ".", "!", "?", "。" };
        protected virtual string[] SplitToWords(string sentence)
        {
            // In default, split sentence to words by character
            // you can override if needed
            char[] charArr = sentence.ToCharArray();
            List<string> words = new List<string>();
            foreach (char c in charArr)
            {
                words.Add(c.ToString());
            }
            return words.ToArray();
        }

        protected virtual string[] VariateAndSplitToWords(string sentence)
        {
            // In default, apply variation(e.g. Phoneme Variation in Korean) and split variated sentence to words by character
            // you can override if needed
            // split sentences with punctuation
            // to fix variating mechanism, you can override Variate method
            string[] unitSentences = Regex.Split(sentence, @"([^\w\s])");

            List<string> newSentences = new List<string>();

            foreach (string unitSentence in unitSentences)
            {
                if (!string.IsNullOrEmpty(unitSentence)) 
                {
                    newSentences.Add(Variate(unitSentence));
                }
            }

            return SplitToWords(string.Join("", newSentences));
        }

        protected virtual string Variate(string sentence)
        {
            return sentence;
        }
        public static bool IsNumber(string word)
        {
            return word.All(char.IsDigit);
        }

        public static bool IsPunctuation(string word)
        {
            return word.All(char.IsPunctuation);
        }


        protected virtual string ToPhoneme(string word, out bool isEditable, out ProsodyType prosodyType)
        {
            if (word.Trim().Equals(string.Empty))
            {

                isEditable = false;
                prosodyType = ProsodyType.None;
                return string.Empty;
            }
            isEditable = true;
            prosodyType = ProsodyType.Undefined;
            return word;
        }

        
        public Task PhonemizeAsync(string sentence, LineBoxView l, bool ApplyToCurrentEdit=true)
        {
            return PhonemizeAsync(sentence, l, DispatcherPriority.ApplicationIdle, ApplyToCurrentEdit);
        }

        public Task GenerateIPAAsync(LineBoxView l)
        {

            return GenerateIPAAsync(l, DispatcherPriority.ApplicationIdle);
        }

        public async Task<string> ConvertToIPA(string sentence, DispatcherPriority dispatcherPriority)
            // used in Datapreprocesswindow
        {
            string ToIPA = sentence.Trim();
            string[] words = SplitToWords(sentence.Trim());
            string[] variatedWords = VariateAndSplitToWords(sentence.Trim());
            List<string> IPAPhonemes = new List<string>();
            bool _ = false;
            ProsodyType prosodyType = ProsodyType.None;
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                IPAPhonemes = new string[variatedWords.Length].ToList();
                bool divideWord = false;
                var wordTasks = variatedWords
                    .Select(async (word, index) => await Task.Run(()=>
                    {
                        string phoneme = ToPhoneme(word, out _, out prosodyType);
                        if (words.Length != variatedWords.Length)
                        {
                            Log.Error($"[ConvertToIPA: Variated Sentence({words.Length})] - [Sentence length({variatedWords.Length})] mismatch");
                            if (UseWordDivider && phoneme.Trim() == string.Empty)
                            {
                                IPAPhonemes[index] = " ";
                                
                            }
                            else
                            {
                                if (index == 0)
                                {
                                    IPAPhonemes[index] = IPAConverter.ConvertToIPA(phoneme.Trim(), true);
                                }
                                else
                                {
                                    IPAPhonemes[index] = IPAConverter.ConvertToIPA(phoneme.Trim(), false);
                                }

                            }
                        }
                        else
                        {
                            if (UseWordDivider && phoneme.Trim() == string.Empty)
                            {
                                IPAPhonemes[index] = " ";
                            }
                            else
                            {
                                if (index == 0)
                                {
                                    IPAPhonemes[index] = IPAConverter.ConvertToIPA(phoneme.Trim(), true);
                                }
                                else
                                {
                                    IPAPhonemes[index] = IPAConverter.ConvertToIPA(phoneme.Trim(), false);
                                }
                            }
                        }

                        divideWord = true;


                    }));

                await Task.WhenAll(wordTasks);

            }, dispatcherPriority);
            // add bos eos
            string IPAString = string.Join("\t", IPAPhonemes);
            string[] unitSentences = Regex.Split(IPAString, @"([^\w\s])");



            List<string> IPAStringTokensAdded = new List<string>();
            IPAStringTokensAdded.Add("<pad>");
            int i = 0;
            foreach (string unitSentence in unitSentences)
            {
                if (!string.IsNullOrEmpty(unitSentence))
                {
                    if ( !IsPunctuation(unitSentence) && 
                        ( i == 0 || i-1 > 0 && endPuncs.Contains(unitSentences[i-1])))
                    {
                        IPAStringTokensAdded.Add("<bos>");

                    }
                    IPAStringTokensAdded.Add(unitSentence);
                    if (endPuncs.Contains(unitSentence))
                    {
                        IPAStringTokensAdded.Add("<eos>");
                    }
                }
                ++i;

            }
            IPAStringTokensAdded.Add("<pad>");

            return string.Join("\t",
                   string.Join(" ", string.Join("\t", IPAStringTokensAdded)
                   .Split(" ", StringSplitOptions.RemoveEmptyEntries))
                   .Replace(" ", "<space>")
                   .Split("\t", StringSplitOptions.RemoveEmptyEntries));

        }


        public async Task PhonemizeAsync(string sentence, LineBoxView l, DispatcherPriority dispatcherPriority, bool ApplyToCurrentEdit= true)
        {
            if (string.IsNullOrEmpty(sentence))
            {
                return;
            }
            sentence = sentence.Trim();
            if (l != null)
            {
                if (!l.ShouldPhonemize)
                {
                    return;
                }
                if (l.DeActivatePhonemizer)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (ApplyToCurrentEdit)
                        {
                            l.UpdateMResultsCollection();
                        }
                        
                    }, dispatcherPriority);
                    return;
                }
            }
            
            //Log.Information("Phonemize: {sentence}", sentence);

            //int unitWidth = 64;
            //int unitHeight = 48;
            //int wordBorderWidth = 1;
            bool editable = true;

            ProsodyType prosodyType = ProsodyType.None;
            string[] words = SplitToWords(sentence.Trim());
            string[] variatedWords = VariateAndSplitToWords(sentence.Trim());

            MResultPrototype[] results = new MResultPrototype[variatedWords.Length];

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                IPAPhonemes.Clear();


                var wordTasks = variatedWords
                    .Select(async (word, index) => 
                    {

                        string phoneme =  ToPhoneme(word, out editable, out prosodyType);
                        if (index != 0 && variatedWords[index-1].Trim().Equals(string.Empty) && ! word.Trim().Equals(string.Empty))
                        {
                            if (prosodyType == ProsodyType.Undefined)
                            {
                                prosodyType = ProsodyType.Space;
                            }
                            results[index] = new MResultPrototype(words[index].Trim(), phoneme.Trim(), editable, prosodyType);
                        }
                        else
                        {
                            if (word.Trim().Equals(string.Empty))
                            {
                                phoneme = string.Empty;
                                results[index] = new MResultPrototype(word, phoneme.Trim(), editable, ProsodyType.None);
                            }
                            else if (words.Length != variatedWords.Length)
                            {
                                Log.Error($"[Variated Sentence({words.Length})] - [Sentence length({variatedWords.Length})] mismatch");

                                results[index] = new MResultPrototype(variatedWords[index].Trim(), phoneme.Trim(), editable, ProsodyType.Undefined);
                            }
                            else
                            {
                                results[index] = new MResultPrototype(words[index].Trim(), phoneme.Trim(), editable, ProsodyType.Undefined);
                            }
                        }
                        
                           

                    });

                await Task.WhenAll(wordTasks);
                SetUpProsody(l, results.ToList());
                
            }, dispatcherPriority);
            //Log.Debug("DOne");
        }
        public static MResultPrototype[] SetUpProsody(LineBoxView l, List<MResultPrototype> results, bool ApplyToCurrentEdit=true)
        {
            List<MResultPrototype> mResults = new List<MResultPrototype>();

            if (l != null)
            {
                
                MResultPrototype prev = null;
                MResultPrototype next = null;

                for (int i = 0; i < results.Count; ++i)
                {
                    MResultPrototype mResultPrototype = results[i];  
                    if (mResultPrototype.Word.Trim() == string.Empty)
                    {
                        mResultPrototype.IsEditable = false;
                    }

                    if (i == results.Count - 1)
                    {
                        Log.Debug("Last");
                        next = null;
                    }
                    else
                    {
                        next = results[i + 1];
                    }
                    if (i == 0)
                    {
                        mResultPrototype.ProsodyType = (int)ProsodyType.Bos;
                    }
                    else
                    {
                        if (prev is not null)
                        {
                            if (IsPunctuation(prev.Phoneme) && endPuncs.Contains(prev.Phoneme) && mResultPrototype.Phoneme is not null && !mResultPrototype.Phoneme.Trim().Equals(string.Empty))
                            {
                                mResultPrototype.ProsodyType = (int)ProsodyType.Bos;
                            }
                        }
                    }

                    if (next is not null && IsPunctuation(next.Phoneme) && endPuncs.Contains(next.Phoneme))
                    {
                        //Log.Debug($"Next is punctuation: {next.Phoneme}");
                        //Log.Debug($"Current: {mResultPrototype.Phoneme}");
                        mResultPrototype.ProsodyType = (int)ProsodyType.Eos;
                    }
                    if (i == results.Count - 1 && prev is not null && prev.ProsodyType != (int)ProsodyType.Eos)
                    {

                        mResultPrototype.ProsodyType = (int)ProsodyType.Eos;
                    }


                    if (mResultPrototype.ProsodyType == (int)ProsodyType.Undefined) // if not set
                    {
                        mResultPrototype.ProsodyType = (int)ProsodyType.None;
                    }

                    mResults.Add(mResultPrototype);


                    prev = mResultPrototype;

                }

                
                if (ApplyToCurrentEdit)
                {
                    List<MResult> mResultsFinal = new List<MResult> ();
                    foreach (MResultPrototype mp in mResults)
                    {
                        mResultsFinal.Add(new MResult(mp, l));
                    }
                    l.MResultsCollection = new ObservableCollection<MResult>(mResultsFinal);
                    l.UpdateMResultsCollection();
                }
            }
            return mResults.ToArray();
        }
        
        public string ApplyProsody(string phoneme, ProsodyType prosodyType, bool addPadLeft=false, bool addPadRight=false)
        {
            string res;
            switch (prosodyType)
            {
                // TODO add pad
                case ProsodyType.Undefined:
                    res = string.Empty; // do not pronounce
                    break;
                case ProsodyType.Bos:
                    res = $"<bos>\t{phoneme}";
                    break;
                case ProsodyType.Eos:
                    res = $"{phoneme}\t<eos>";
                    break;
                case ProsodyType.Space:
                    res= $"<space>\t{phoneme}";
                    break;
                case ProsodyType.None:
                    res = phoneme;
                    break;
                default:
                    res = string.Empty;
                    break;

            }

            if (addPadLeft)
            {
                res = "<pad>\t" + res;
            }
            if (addPadRight)
            {
                res = res + "\t<pad>";
            }
            return res;
        }
        public async Task GenerateIPAAsync(LineBoxView l, DispatcherPriority dispatcherPriority)
        {
            //Log.Debug("Generating IPA with current phonemes");
            if (l is null)
            {
                return;
            }
            if (l.MResultsCollection.Count == 0)
            {
                return;
            }

            

            int unitWidth = 64;
            int unitHeight = 48;
            int wordBorderWidth = 1;
            bool editable = true;
            
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                string[] IPAPhonemes = new string[l.MResultsCollection.Count];

                bool divideWord = false;
                var phonemeTasks = l.MResultsCollection
                    .Select(async (mResult, index) => await Task.Run(() =>
                    {

                            string phoneme = mResult.mTextBoxEditor.CurrentScript;

                            if (UseWordDivider && (phoneme == null || phoneme.Trim() == string.Empty))
                            {
                                string phone = "";
                         
                                IPAPhonemes[index] = ApplyProsody(phone, (ProsodyType)mResult.Prosody);
                            }
                            else
                            {
                                if (index == 0)
                                {
                                    string phone = IPAConverter.ConvertToIPA(phoneme.Trim(), true);
                                    IPAPhonemes[index] = ApplyProsody(phone, (ProsodyType)mResult.Prosody, true);
                                }
                                else if (index == l.MResultsCollection.Count - 1)
                                {
                                    string phone = IPAConverter.ConvertToIPA(phoneme.Trim(), false);
                                    IPAPhonemes[index] = ApplyProsody(phone, (ProsodyType)mResult.Prosody, false, true);
                                }
                                else
                                {
                                    string phone = IPAConverter.ConvertToIPA(phoneme.Trim(), false);
                                    IPAPhonemes[index] = ApplyProsody(phone, (ProsodyType)mResult.Prosody);
                                }
                            }
                      

                        divideWord = true;


                    }));

                await Task.WhenAll(phonemeTasks);
                if (l != null)
                {
                    
                    

                    l.IPAText = string.Join("\t",
                                string.Join(" ", string.Join("\t", IPAPhonemes)
                                .Split(" ", StringSplitOptions.RemoveEmptyEntries))
                                .Replace(" ", "<space>")
                                .Split("\t", StringSplitOptions.RemoveEmptyEntries));
                    Log.Debug($"IPA generated: {l.IPAText}");
                   
                }

            }, dispatcherPriority);
            //Log.Debug("Generated IPA with current phonemes");
        }
    }
}
