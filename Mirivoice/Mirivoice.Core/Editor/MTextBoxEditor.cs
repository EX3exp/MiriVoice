using Mirivoice.Commands;
using Mirivoice.ViewModels;
using ReactiveUI;
using Serilog;
using System.ComponentModel;

namespace Mirivoice.Mirivoice.Core.Editor
{

    public partial class MTextBoxEditor : ReactiveObject, INotifyPropertyChanged
    {
        string _currentScript;
        VoicerSelectingViewModelBase v;
        bool IsFirstExec = true;

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
        }
        public TextBoxEditCommand textBoxEditCommand;
        public MTextBoxEditor(VoicerSelectingViewModelBase v, string _currentScript)
        {
            //Log.Debug("MTextBoxEditor Constructor");
            this.v = v;
            
            this._currentScript = _currentScript;
            
        }

        public bool DonotExecCommand = false;

        public string CurrentScript
        {
            get
            {
                return _currentScript;
            }
            set
            {
                if (IsFirstExec)
                {
                    textBoxEditCommand = new TextBoxEditCommand(v);
                    IsFirstExec = false;
                }

                
                this.RaiseAndSetIfChanged(ref _currentScript, value);
                if (!DonotExecCommand) 
                    MainManager.Instance.cmd.ExecuteCommand(textBoxEditCommand);
                OnPropertyChanged(nameof(CurrentScript));
            }
        }


    }
}
