using Avalonia.Threading;
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

namespace Mirivoice.Mirivoice.Core.Format
{
    [YamlObject]
    public partial class MResultPrototype
    {
        public string Word { get; set; }
        public string Phoneme { get; set; }
        public bool IsEditable { get; set; }

        [YamlConstructor]
        public MResultPrototype() { }
        public MResultPrototype(MResult mResult)
        {
            Word = mResult.Word;
            Phoneme = mResult.mTextBoxEditor.CurrentScript;
            IsEditable = mResult.IsEditable;
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

        [YamlConstructor]
        public Mrp()
        {
            
        }
        public Mrp(MainViewModel v)
        {
            Version = new Version(0, 1); // mirivoice 1.0.0
            mLines = v.LineBoxCollection.Select(l => new MLinePrototype(l)).ToArray();
            MultEditScript = v.mTextBoxEditor.CurrentScript;
            DefaultVoicerName = v.voicerSelector.CurrentVoicer.Info.Name;
        }

        public async Task Load(MainViewModel v)
        {
            Log.Information("Loading Project");
            v.project = this;

            LineBoxView[] lines = new LineBoxView[mLines.Length];

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var mLineTasks =this.mLines
                    .Select(async (l, index) =>
                    {

                        int voicerIndex = MainManager.Instance.VoicerM.FindVoicerIndex(
                        MainManager.Instance.VoicerM.FindVoicerWithNameAndLangCodeAndUUID(l.voicerName, l.langCode, l.voicerUuid)
                        );
                        int metaIndex = l.voicerStyleId;
                        lines[index] = new LineBoxView(l, v, index, voicerIndex, metaIndex);

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
                }
                else
                {
                    v.voicerSelector.CurrentDefaultVoicerIndex = MainManager.Instance.VoicerM.FindVoicerIndex(defVoicer);
                    v.voicerSelector.Voicers[v.voicerSelector.CurrentDefaultVoicerIndex].CurrentVoicerMeta =
                        v.voicerSelector.Voicers[v.voicerSelector.CurrentDefaultVoicerIndex].VoicerMetaCollection[this.DefaultVoicerStyleId];
                }

                

                MainManager.Instance.cmd.ProjectOpened();
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
