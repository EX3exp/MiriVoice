using Mirivoice.Commands;
using Mirivoice.Engines;
using Mirivoice.Engines.TTS;
using Mirivoice.Views;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using VYaml.Annotations;

namespace Mirivoice.Mirivoice.Core.Format
{
    [YamlObject]
    public partial class VoicerInfo 
    {

        public string Name { get; set; } = "";
        public string Nickname { get; set; } = "";
        public string Description { get; set; } = "";
        public string Voice { get; set; } = "";
        public string Author { get; set; } = "";
        public string Engineer { get; set; } = "";
        public string Illustrator { get; set; } = "";
        public string Web { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Portrait { get; set; } = "";
        
        public string Type { get; set; } = "";
        public string ConfigPath { get; set; } = "";
        public string UpdateUrlGithub { get; set; } = "";

        public string LangCode { get; set; } = "";
        public string Version { get; set; } = "1.0.0";
        public string uuid { get; set; } = "";

        public VoicerMeta[] VoicerMetas { get; set; } = new VoicerMeta[1] { new VoicerMeta() };

        public string Readme { get; set; } = "";
    }

    public class Voicer : ReactiveObject, INotifyPropertyChanged
    {

        public VoicerInfo Info { get; set; }

        public ObservableCollection<VoicerMeta> VoicerMetaCollection { get; set; }
        public bool NotProcessingSetVoicerMetaCommand = false;
        //public int DefaultVoicerMetaIndex { get; set; }

        private VoicerMeta _currentVoicerMeta;
        private VoicerMeta lastVoicerMeta;

        private bool Undobackuped = false;

        public string RootPath { get; set; } = "";
        BaseEngine Engine { get; set; }
        public void SetRootPath(string rootPath)
        {
            RootPath = rootPath;
            if (Info.Type == VoicerMetaType.VITS2.ToString())
            {
                Engine = new EngineVITS2();
            }

            Engine.Init(this);
        }

        public void Inference(string ipaText, string cacheFilePath, LineBoxView l)
        {
            //Log.Debug("[Infer Started] --- ipaText");
            int spkid = CurrentVoicerMeta.SpeakerId;
            Inference(ipaText, cacheFilePath, l, spkid);
        }

        public void Inference(string ipaText, string cacheFilePath, LineBoxView l, int sid)
        {
            //Log.Debug("[Infer Started] --- ipaText");
            Engine.Inference(ipaText, cacheFilePath, sid);
            if (l != null)
            {
                l.IsCacheIsVaild = true;
            }
        }




        public VoicerMeta CurrentVoicerMeta
        {
            get
            {
                return _currentVoicerMeta;
            }
            set
            {
                if (value is not null)
                {
                    

                    lastVoicerMeta = _currentVoicerMeta;

                    Log.Debug("CurrentVoicerMeta: {value}", value);

                    Log.Debug($"CurrentVoicerMeta: {value.Style}");
                    Log.Debug($"lastVoicerMeta: {lastVoicerMeta.Style}");
                    if (!NotProcessingSetVoicerMetaCommand)
                    {
                        if (!Undobackuped)
                        {
                            Log.Debug($"Backup: {lastVoicerMeta}");
                            SetVoicerMetaCommand.Backup(lastVoicerMeta);
                            Undobackuped = true;
                        }
                        
                        MainManager.Instance.cmd.ExecuteCommand(SetVoicerMetaCommand);
                        Undobackuped = false;
                    }
                    else
                    {
                        NotProcessingSetVoicerMetaCommand = false;
                    }
                    this.RaiseAndSetIfChanged(ref _currentVoicerMeta, value);
                    this.RefreshNickAndStyle();
                    OnPropertyChanged(nameof(NickAndStyle));
                    OnPropertyChanged(nameof(CurrentVoicerMeta));

                }
            }
        }

        public Voicer(VoicerInfo voicerInfo)
        {
            if (voicerInfo is null)
            {
                throw new ArgumentNullException(nameof(voicerInfo));
            }
            this.Info = voicerInfo;
            this.VoicerMetaCollection = new ObservableCollection<VoicerMeta>(voicerInfo.VoicerMetas);
            this._currentVoicerMeta = VoicerMetaCollection[0];
            this.lastVoicerMeta = VoicerMetaCollection[0];
            this._nickAndStyle = ToString();

            
            SetCommand();
        }

        public void SetDefaultVoicerMeta(int DefaultVoicerMetaIndex)
        {
            _currentVoicerMeta = VoicerMetaCollection[DefaultVoicerMetaIndex];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            //Log.Debug("[Property Changed] {propertyName}", propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MementoCommand<VoicerMeta> SetVoicerMetaCommand { get; set; }
        SetVoicerMetaOriginator setVoicerMetaOriginator;

        private void SetCommand()
        {
            setVoicerMetaOriginator = new SetVoicerMetaOriginator(this, ref _currentVoicerMeta);
            SetVoicerMetaCommand = new MementoCommand<VoicerMeta>(setVoicerMetaOriginator);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("[");
            sb.Append(Info.Nickname);
            sb.Append("] (");
            sb.Append(CurrentVoicerMeta.Style);
            sb.Append(")");
            return sb.ToString();
        }

        private string _nickAndStyle;
        public string NickAndStyle
        {
            get
            {
                return _nickAndStyle;

            }
            set 
            { 
                if (value is not null)
                {
                    _nickAndStyle = value;
                    OnPropertyChanged(nameof(NickAndStyle));
                }
            }
        }

        public void RefreshNickAndStyle()
        {
            NickAndStyle = ToString();
        }
    }

    public class SetVoicerMetaOriginator : MOriginator<VoicerMeta>
    {
        private Voicer v;
        private VoicerMeta meta;
        public SetVoicerMetaOriginator(Voicer v, ref VoicerMeta meta) : base(ref meta)
        {
            this.meta = meta;
            this.v = v;
           
        }

        public override void UpdateProperties()
        {
            //Log.Debug($"[Updating Properties] -- {obj.Style}");
            v.NotProcessingSetVoicerMetaCommand = true; // prevent recursion loop
            v.CurrentVoicerMeta = obj;

        }
    }
}
