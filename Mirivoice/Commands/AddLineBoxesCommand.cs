using Mirivoice.Mirivoice.Core;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mirivoice.Commands
{
    public class AddLineBoxesCommand : ICommand
    {
        private MainViewModel v;
        private int InitialEndOfLineBoxCollection;

        private string script = string.Empty;
        int DefaultVoicerOriginal;
        int DefaultVoicerMetaOriginal;
        

        public AddLineBoxesCommand(MainViewModel mainViewModel)
        {
            v = mainViewModel;
            InitialEndOfLineBoxCollection = v.LineBoxCollection.Count - 1; // if undo, remove all lineboxes after this index

            DefaultVoicerOriginal = MainManager.Instance.DefaultVoicerIndex;
            DefaultVoicerMetaOriginal = MainManager.Instance.DefaultMetaIndex;

            script = v.mTextBoxEditor.CurrentScript;
            InitialEndOfLineBoxCollection = v.LineBoxCollection.Count - 1; // if undo, remove all lineboxes after this index
        }

        public void Execute(bool isRedoing)
        {
            string pattern = @"\[(?<nickname>[^\]]+)\](?:\s*\((?<style>[^\)]+)\))?";
            var regex = new Regex(pattern);

            // split texts
            string[] lines = script.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            List<string> results = new List<string>(); // for logging
            bool isEmptyLine = false;
            DefaultVoicerOriginal = MainManager.Instance.DefaultVoicerIndex;
            DefaultVoicerMetaOriginal = MainManager.Instance.DefaultMetaIndex;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (!isEmptyLine)
                    {
                        results.Add(string.Empty);
                        isEmptyLine = true;
                    }
                    continue;
                }

                isEmptyLine = false;

                // pattern match
                var match = regex.Match(line);
                if (match.Success)
                {
                    string nickname = match.Groups["nickname"].Value;
                    string style = match.Groups["style"].Value;

                    SetDefVoicer(nickname, style);
                }
                else
                {

                    AddLineBox(line.Trim());
                    results.Add(line.Trim());
                }
            }


            MainManager.Instance.DefaultVoicerIndex = DefaultVoicerOriginal;
            MainManager.Instance.DefaultMetaIndex = DefaultVoicerMetaOriginal;
        }

        public void SetScript(string newscript) // should be called when script is changed
        {
            script = newscript;
            InitialEndOfLineBoxCollection = v.LineBoxCollection.Count - 1; // if undo, remove all lineboxes after this index
            
        }


        public void UnExecute()
        {
            for (int i = v.LineBoxCollection.Count - 1; i > InitialEndOfLineBoxCollection; i--)
            {


                if (v.CurrentLineBox == v.LineBoxCollection[i])
                {
                    v.CurrentSingleLineEditor = null;
                    v.MResultsCollection.Clear();
                }
                if (v.CurrentEditIndex == 1)
                {
                    v.CurrentEdit = null;
                    v.OnPropertyChanged(nameof(v.CurrentEdit));
                }
                v.OnPropertyChanged(nameof(v.CurrentSingleLineEditor));
                v.LineBoxCollection.RemoveAt(i);
            }
        }



        private void SetDefVoicer(string nick, string style)
        {
            //Log.Debug($"SetDefVoicer_Event -- {nick} {style}");
            List<Voicer> voicers = MainManager.Instance.GetVoicersCollectionNew().ToList();
            int voicerIndex = voicers.FindIndex(x => x.Info.Nickname == nick);

            if (voicerIndex == -1)
            {
                Log.Warning($"Non-Existing Nickname: {nick}. Ignoring current line.");
                return;
            }

            MainManager.Instance.DefaultVoicerIndex = voicerIndex;

            int styleIndex = voicers[voicerIndex].VoicerMetaCollection.ToList().FindIndex(x => x.Style == style);
            if (styleIndex == -1)
            {
                Log.Warning($"Non-Existing Style: {style}.");
                styleIndex = 0;
            }
            MainManager.Instance.DefaultMetaIndex = styleIndex;

        }

        private void AddLineBox(string line)
        {
            //Log.Debug($"AddLineBox_Event -- {line}");
            var lineBox = new LineBoxView(v, line);
            int LineNoToBeAdded = v.LineBoxCollection.Count + 1;

            lineBox.viewModel.SetLineNo(LineNoToBeAdded);

            v.LineBoxCollection.Add(lineBox);
            lineBox.ScrollToEnd();
        }

    }
}
