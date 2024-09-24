using Mirivoice.Commands;
using Mirivoice.Engines;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;
using ReactiveUI;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

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
                    v._currentDefaultVoicerIndex = Voicers.IndexOf(value);
                    OnPropertyChanged(nameof(CurrentDefaultVoicerIndex));

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

        /// <summary>
        /// can be used when new voicer is ADDED
        /// (do not use when voicer is removed)
        /// </summary>
        public void UpdateVoicerCollection()
        {
            ObservableCollection<Voicer> _voicers = MainManager.Instance.GetVoicersCollectionNew();
            int offset = _voicers.Count - Voicers.Count - 1; // this amount of voicers will be added
            if (offset < 0)
            {
                return;
            }
            for (int i = offset; i < _voicers.Count; i++)
            {
                Voicers.Add(_voicers[i]);
            }

        }

        bool Undobackuped = false;

        public int CurrentDefaultVoicerIndex
        {
            get
            {
                if (_currentDefaultVoicerIndex == -1)
                {
                    this.RaiseAndSetIfChanged(ref _currentDefaultVoicerIndex, 0);
                }
                return _currentDefaultVoicerIndex;
            }
            set
            {
                if (value == -1)
                {
                    return;
                }
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
