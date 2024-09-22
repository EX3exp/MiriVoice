using Mirivoice.Commands;
using Mirivoice.Engines;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;
using ReactiveUI;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Mirivoice.Mirivoice.Core.Editor
{
    public class VoicerSelector : ReactiveObject, INotifyPropertyChanged
    {
        int _currentDefaultVoicerIndex;
        int lastDefaultVoicerIndex;
        VoicerSelectingViewModelBase v;
        private Voicer _currentVoicer;

        public MementoCommand<int> SetDefVoicerCommand { get; set; }
        SetDefVoicerOriginator setDefVoicerOriginator;
        public ObservableCollection<VoicerMeta> CurrentVoicerMetaCollection { get; set; } = new ObservableCollection<VoicerMeta>();

        public ObservableCollection<Voicer> Voicers { get; set; }

        public Voicer CurrentVoicer
        {
            get => _currentVoicer;
            set
            {
                if (value is not null)
                {
                    this.RaiseAndSetIfChanged(ref _currentVoicer, value);
                    _currentVoicer.RefreshNickAndStyle();
                    OnPropertyChanged(nameof(CurrentVoicer));
                    
                }
                
            }
        }

        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            //Log.Debug("[Property Changed]: {propertyName}", propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public VoicerSelector(VoicerSelectingViewModelBase v, ref int _currentDefaultVoicerIndex, ref int lastDefaultVoicerIndex)
        {
            //Log.Debug("VoicerSelector Constructor");
            this.v = v;
            Voicers = MainManager.Instance.GetVoicersCollectionNew();
            this._currentDefaultVoicerIndex = _currentDefaultVoicerIndex;
            this._currentVoicer = Voicers[_currentDefaultVoicerIndex];
            setDefVoicerOriginator = new SetDefVoicerOriginator(v, ref _currentDefaultVoicerIndex);
            SetDefVoicerCommand = new MementoCommand<int>(setDefVoicerOriginator);
        }

        
        bool Undobackuped = false;

        public int CurrentDefaultVoicerIndex
        {
            get
            {
                return _currentDefaultVoicerIndex;
            }
            set
            {
                
                lastDefaultVoicerIndex = _currentDefaultVoicerIndex;

                //Log.Debug("CurrentDefaultVoicerIndex: {value}", value);

                //Log.Debug($"LastDefaultVoicerIndex: {lastDefaultVoicerIndex}");
                //Log.Debug($"NotProCessingCommand: {v.NotProcessingSetDefVoicerCommand}");


                if (!v.NotProcessingSetDefVoicerCommand)
                {
                    if (!Undobackuped)
                    {
                        //Log.Debug($"backup : {lastDefaultVoicerIndex}");

                        SetDefVoicerCommand.Backup(lastDefaultVoicerIndex);
                        Undobackuped = true;
                    }
                    MainManager.Instance.cmd.ExecuteCommand(SetDefVoicerCommand);
                    
                    Undobackuped = false;
                }
                else
                {
                    v.NotProcessingSetDefVoicerCommand = false;

                }
                this.RaiseAndSetIfChanged(ref _currentDefaultVoicerIndex, value);
                _currentVoicer = Voicers[_currentDefaultVoicerIndex];
                _currentVoicer.RefreshNickAndStyle();
                v.OnVoicerChanged(_currentVoicer);
                OnPropertyChanged(nameof(CurrentVoicer));
                OnPropertyChanged(nameof(CurrentDefaultVoicerIndex));


            }
        }
        

    }

    
}
