using Avalonia.Threading;
using Mirivoice.Engines;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using R3;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VYaml.Annotations;
using VYaml.Serialization;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers;

namespace Mirivoice.Mirivoice.Core.Format
{
    [YamlObject]
    public partial class MResultPrototype
    {
        public string Word { get; set; }
        public string Phoneme { get; set; }
        public bool IsEditable { get; set; }
        public int ProsodyType { get; set; } = -1;

        [YamlConstructor]
        public MResultPrototype() { }
        public MResultPrototype(MResult mResult)
        {
            Word = mResult.Word;
            Phoneme = mResult.mTextBoxEditor.CurrentScript;
            IsEditable = mResult.IsEditable;
            ProsodyType = mResult.Prosody;
        }

        public MResultPrototype(string word, string phoneme, bool isEditable, ProsodyType prosodyType )
        {
            Word = word;
            Phoneme = phoneme;
            IsEditable = isEditable;
            ProsodyType = (int)prosodyType;
        }
    }

    [YamlObject]
    public partial class MLinePrototype
    {
        public string LineText { get; set; } = "";
        public MResultPrototype[] PhonemeEdit { get; set; }

        public string voicerUuid { get; set; }
        public string langCode { get; set; }
        public string voicerName { get; set; }

        public int voicerStyleId { get; set; }
        public string IPAText { get; set; }
        public string cacheName { get; set; }
        public MExpressionsWrapper Exp { get; set; } = new MExpressionsWrapper();

        [YamlConstructor]
        public MLinePrototype()
        {

        }
        public MLinePrototype(LineBoxView l)
        {
            LineText = l.viewModel.LineText;
            PhonemeEdit = l.MResultsCollection.Select(m => new MResultPrototype(m)).ToArray();
            this.langCode = l.viewModel.LangCode;
            voicerName = l.viewModel.voicerSelector.CurrentVoicer.Info.Name;
            voicerStyleId = l.viewModel.voicerSelector.CurrentVoicer.CurrentVoicerMeta.SpeakerId;
            this.IPAText = l.IPAText;
            voicerUuid = l.viewModel.voicerSelector.CurrentVoicer.Info.uuid;
            if (l.CurrentCacheName != null && l.CurrentCacheName != string.Empty )
            {
                cacheName = l.CurrentCacheName;
            }
            this.Exp = l.Exp;
        }
    }

    [YamlObject]    
    public partial class Mrp
    {
        public Version Version { get; set; } = new Version(0, 1);

        public MLinePrototype[] mLines { get; set; } = new MLinePrototype[0];
        public string MultEditScript { get; set; } = "";
        public string DefaultVoicerName { get; set; } = "";
        public int DefaultVoicerStyleId { get; set; } = 0;
        [YamlIgnore]
        public Version CurrentVersion = new Version(0, 2);

        [YamlConstructor]
        public Mrp()
        {
            
        }
        public Mrp(MainViewModel v)
        {
            Version = CurrentVersion;
            mLines = v.LineBoxCollection.Select(l => new MLinePrototype(l)).ToArray();
            MultEditScript = v.mTextBoxEditor.CurrentScript;
            DefaultVoicerName = v.voicerSelector.CurrentVoicer.Info.Name;
            DefaultVoicerStyleId = v.voicerSelector.CurrentVoicer.CurrentVoicerMeta.SpeakerId;
        }

        

        public async Task Load(MainViewModel v)
        {
            Log.Information("Loading Project");
            
            if (Version > CurrentVersion)
            {
                var res = await v.ShowConfirmWindow("menu.files.open.upgrade");
                return;
            }
            v.project = this;

            if (Version < CurrentVersion)
            {
                Log.Information($"Upgrading project from {Version} to {CurrentVersion}.");
            }
            if (Version < new Version(0, 2))
            {
                foreach (var m in mLines)
                {
                    m.PhonemeEdit = BasePhonemizer.SetUpProsody(null, m.PhonemeEdit.ToList(), false);
                    m.Exp = new MExpressionsWrapper();
                }
            }

            LineBoxView[] lines = new LineBoxView[mLines.Length];

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var mLineTasks =this.mLines
                    .Select(async (l, index) =>
                    {
                        int voicerIndex;
                        Voicer voicer = MainManager.Instance.VoicerM.FindVoicerWithNameAndLangCodeAndUUID(l.voicerName, l.langCode, l.voicerUuid);
                        if (voicer != null)
                        {
                             voicerIndex = MainManager.Instance.VoicerM.FindVoicerIndex(
                                voicer
                            );
                        }
                        else
                        {
                            Log.Information($"[Mrp load] voicer not exists, use first voicer instead.");
                            voicerIndex = 0;
                            MainManager.Instance.cmd.ChangedToDefVoicer = true;
                        }
                        int spkid = l.voicerStyleId;
                        int metaIndex = 0;

                        v.voicerSelector.UpdateVoicerCollection();
                        Log.Debug($"voicerIndex = {voicerIndex}, Voicers = {v.voicerSelector.Voicers}");
                        foreach (VoicerMeta v_ in v.voicerSelector.Voicers[voicerIndex].VoicerMetaCollection)
                        {
                            
                            if (v_.SpeakerId == spkid)
                            {
                                break;
                            }
                            metaIndex++;
                        }
                        lines[index] = new LineBoxView(l, v, index, voicerIndex, metaIndex, false);

                    });

                await Task.WhenAll(mLineTasks);

                v.mTextBoxEditor.CurrentScript = this.MultEditScript;

                if (mLines.Length == 0)
                {
                    Log.Debug("Opening Empty Project");
                    return;
                }

                // Default Voicer 처리
                Voicer defVoicer = MainManager.Instance.VoicerM.FindVoicerWithNameAndLangCodeAndUUID(this.DefaultVoicerName, this.mLines[0].langCode, this.mLines[0].voicerUuid);
                if (defVoicer == null)
                {
                    Log.Warning($"Non-Existing Voicer: {this.DefaultVoicerName}. Using Current Default Voicer instead.");
                    MainManager.Instance.cmd.ChangedToDefVoicer = true;
                }
                else
                {
                    v.voicerSelector.CurrentDefaultVoicerIndex = MainManager.Instance.VoicerM.FindVoicerIndex(defVoicer);
                    v.voicerSelector.Voicers[v.voicerSelector.CurrentDefaultVoicerIndex].CurrentVoicerMeta =
                        v.voicerSelector.Voicers[v.voicerSelector.CurrentDefaultVoicerIndex].VoicerMetaCollection.FirstOrDefault(m => m.SpeakerId == this.DefaultVoicerStyleId);
                }

                
            }, DispatcherPriority.Send);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                v.LineBoxCollection = new ObservableCollection<LineBoxView>(lines);
                v.OnPropertyChanged(nameof(v.LineBoxCollection));
                v.OnPropertyChanged(nameof(v.voicerSelector));
            }, DispatcherPriority.Normal);
        }



        public void Save(string filePath)
        {

            try
            {
                WriteYaml(filePath);
                MainManager.Instance.cmd.ProjectSaved();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to save mrp: {filePath}", $"<translate:errors.failed.save>: {filePath}", ex);

            }
        }
        void WriteYaml(string yamlPath)
        {
            var utf8Yaml = YamlSerializer.SerializeToString(this);
            File.WriteAllText(yamlPath, utf8Yaml);
        }
        
    }
}
