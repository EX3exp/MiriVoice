using Avalonia.Threading;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Mirivoice.Plugins.Builtin.IPAConverters;
using Mirivoice.Views;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
        private List<string> endPuncs = new List<string> { ".", "!", "?", "。" };
        protected virtual string[] SplitToWords(string sentence)
        {
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
            // you can override if needed
            // split sentences with punctuation
            string[] unitSentences = Regex.Split(sentence, @"([^\w\s])");

            List<string> newSentences = new List<string>();

            foreach (string unitSentence in unitSentences)
            {
                if (!string.IsNullOrEmpty(unitSentence)) 
                {
                    newSentences.Add(Variate(unitSentence));
                }
            }

            char[] charArr = string.Join("", newSentences).ToCharArray();
            List<string> words = new List<string>();
            foreach (char c in charArr)
            {
                words.Add(c.ToString());
            }
            return words.ToArray();
        }

        protected virtual string Variate(string sentence)
        {
            return sentence;
        }
        public bool IsNumber(string word)
        {
            return word.All(char.IsDigit);
        }

        public bool IsPunctuation(string word)
        {
            return word.All(char.IsPunctuation);
        }


        protected virtual string[] ToPhonemes(string word, out bool isEditable)
        {
            if (word.Trim() == string.Empty)
            {

               isEditable = false;
                return new string[] { string.Empty };
            }
            isEditable = true;
            return new string[] { word }; 
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

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                IPAPhonemes.Clear();
                bool divideWord = false;
                var wordTasks = variatedWords
                    .Select(async (word, index) =>
                    {
                        string phoneme = string.Join("", ToPhonemes(word, out _));
                        if (words.Length != variatedWords.Length)
                        {
                            Log.Error($"[ConvertToIPA: Variated Sentence({words.Length})] - [Sentence length({variatedWords.Length})] mismatch");
                            if (UseWordDivider && phoneme.Trim() == string.Empty)
                            {
                                IPAPhonemes.Add(" ");
                            }
                            else
                            {
                                if (index == 0)
                                {
                                    IPAPhonemes.Add(IPAConverter.ConvertToIPA(phoneme.Trim(), true));
                                }
                                else
                                {
                                    IPAPhonemes.Add(IPAConverter.ConvertToIPA(phoneme.Trim(), false));
                                }

                            }
                        }
                        else
                        {
                            if (UseWordDivider && phoneme.Trim() == string.Empty)
                            {
                                IPAPhonemes.Add(" ");
                            }
                            else
                            {
                                if (index == 0)
                                {
                                    IPAPhonemes.Add(IPAConverter.ConvertToIPA(phoneme.Trim(), true));
                                }
                                else
                                {
                                    IPAPhonemes.Add(IPAConverter.ConvertToIPA(phoneme.Trim(), false));
                                }
                            }
                        }

                        divideWord = true;


                    });

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
                    if ( !IsPunctuation(unitSentence) && ( i == 0 || i-1 > 0 && endPuncs.Contains(unitSentences[i-1])))
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

            int unitWidth = 64;
            int unitHeight = 48;
            int wordBorderWidth = 1;
            bool editable = true;

            
            string[] words = SplitToWords(sentence.Trim());
            string[] variatedWords = VariateAndSplitToWords(sentence.Trim());
            
            List<MResult> results = new List<MResult>();
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                IPAPhonemes.Clear();

                bool divideWord = false;
                var wordTasks = variatedWords
                    .Select(async (word, index)=>
                    {

                        string phoneme = string.Join("", ToPhonemes(word, out editable));
                        if (words.Length != variatedWords.Length)
                            {
                            Log.Error($"[Variated Sentence({words.Length})] - [Sentence length({variatedWords.Length})] mismatch");
                            results.Add(new MResult(word.Trim(), phoneme.Trim(), editable));
                        }
                        else
                        {
                            results.Add(new MResult(words[index].Trim(), phoneme.Trim(), editable));
                        }
                            
                            divideWord = true;

                    });

                await Task.WhenAll(wordTasks);
                if (l != null)
                {
                    l.MResultsCollection = new ObservableCollection<MResult>(results);
                    if (ApplyToCurrentEdit)
                    {
                        l.UpdateMResultsCollection();
                    }
                }
                
            }, dispatcherPriority);
            
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
                IPAPhonemes.Clear();

                bool divideWord = false;
                var phonemeTasks = l.MResultsCollection
                    .Select(async (mResult, index) =>
                    {

                            string phoneme = mResult.mTextBoxEditor.CurrentScript;

                            if (UseWordDivider && (phoneme == null || phoneme.Trim() == string.Empty))
                            {
                                IPAPhonemes.Add(" ");
                            }
                            else
                            {
                                if (index == 0)
                                {
                                    IPAPhonemes.Add(IPAConverter.ConvertToIPA(phoneme.Trim(), true));
                                }
                                else
                                {
                                    IPAPhonemes.Add(IPAConverter.ConvertToIPA(phoneme.Trim(), false));
                                }
                            }
                      

                        divideWord = true;


                    });

                await Task.WhenAll(phonemeTasks);
                if (l != null)
                {
                    // add bos eos
                    string IPAString = string.Join("\t", IPAPhonemes);
                    string[] unitSentences = Regex.Split(IPAString, @"([^\w\s])");



                    List<string> IPAStringTokensAdded = new List<string>();
                    IPAStringTokensAdded.Add("<pad>");
                    foreach (string unitSentence in unitSentences)
                    {
                        if (!string.IsNullOrEmpty(unitSentence))
                        {
                            if (!IsPunctuation(unitSentence))
                            {
                                IPAStringTokensAdded.Add("<bos>");
                            }
                            IPAStringTokensAdded.Add(unitSentence);
                            if (endPuncs.Contains(unitSentence))
                            {
                                IPAStringTokensAdded.Add("<eos>");
                            }
                        }

                    }
                    IPAStringTokensAdded.Add("<pad>");

                    l.IPAText = string.Join("\t",
                                string.Join(" ", string.Join("\t", IPAStringTokensAdded)
                                .Split(" ", StringSplitOptions.RemoveEmptyEntries))
                                .Replace(" ", "<space>")
                                .Split("\t", StringSplitOptions.RemoveEmptyEntries));
                    //Log.Debug($"IPA generated: {l.IPAText}");
                   
                }

            }, dispatcherPriority);
            //Log.Debug("Generated IPA with current phonemes");
        }
    }
}
