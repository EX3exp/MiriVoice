using Mirivoice.Commands;
using Mirivoice.ViewModels;
using ReactiveUI;
using Serilog;
using System.ComponentModel;
using VYaml.Annotations;

namespace Mirivoice.Mirivoice.Core.Editor
{
    
    public partial class MTextBoxEditor : ReactiveObject, INotifyPropertyChanged
    {
        string _currentScript;
        string lastScript;
        VoicerSelectingViewModelBase v;
        bool init = false;

        public MementoCommand<string> TextBoxEditCommand { get; set; }
        TextBoxEditOriginator textBoxEditOriginator;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            //Log.Debug("[Property Changed]: {propertyName}", propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MTextBoxEditor()
        {
            //Log.Debug("MTextBoxEditor Constructor -- default");
            this._currentScript = string.Empty;
            this.lastScript = string.Empty;
            init = false;
        }

        public MTextBoxEditor(VoicerSelectingViewModelBase v, ref string _currentScript, ref string lastScript)
        {
            //Log.Debug("MTextBoxEditor Constructor");
            this.v = v;
            
            this._currentScript = _currentScript;
            this.lastScript = lastScript;
            textBoxEditOriginator = new TextBoxEditOriginator(v, ref _currentScript);
            TextBoxEditCommand = new MementoCommand<string>(textBoxEditOriginator);
            

            init = true;
        }




        bool Undobackuped = false;

        public string CurrentScript
        {
            get
            {
                return _currentScript;
            }
            set
            {
                if (!init)
                {
                    //Log.Debug("MTextBoxEditor not fully initialized");
                    return;
                }
                
                if ( value != null)
                {
                    lastScript = _currentScript;

                    //Log.Debug("CurrentScript: {value}", value);

                    //Log.Debug($"LastScript: {lastScript}");
                    //Log.Debug($"NotProCessingCommand: {v.NotProcessingSetScriptCommand}");


                    if (!v.NotProcessingSetScriptCommand)
                    {
                        if (!Undobackuped)
                        {
                            //Log.Debug($"backup : {lastScript}");

                            TextBoxEditCommand.Backup(lastScript);
                            Undobackuped = true;
                        }
                        MainManager.Instance.cmd.ExecuteCommand(TextBoxEditCommand);

                        Undobackuped = false;
                    }
                    else
                    {
                        v.NotProcessingSetScriptCommand = false;

                    }
                    this.RaiseAndSetIfChanged(ref _currentScript, value);

                    OnPropertyChanged(nameof(CurrentScript));

                }


            }
        }


    }
}
