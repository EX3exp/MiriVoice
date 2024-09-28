using Mirivoice.Commands;
using Mirivoice.Engines;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;
using ReactiveUI;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

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

        struct VoicerUpdate
        {
            public Voicer Voicer;
            public int Index;
            public VoicerUpdate(Voicer voicer, int index)
            {
                Voicer = voicer;
                Index = index;
            }
        }

        // TODO Fix bug : VoicerSelector does not update voicerscollection properly, when voicer is updated by voicerinstaller.
        public void UpdateVoicerCollection()
        {
            ObservableCollection<Voicer> _voicers = MainManager.Instance.GetVoicersCollectionNew();
            List<Voicer> voicersToAdd = new List<Voicer>();
            List<Voicer> voicersToRemove = new List<Voicer>();
            List<VoicerUpdate> voicersToUpdate = new List<VoicerUpdate>();

            foreach (var voicer in _voicers)
            {
                bool newlyInstalled = true;
                bool installedVoicerUpdated = false;
                int updatedVoicerIndex = 0;
                int index = 0;
                foreach (var voicer2 in Voicers)
                {
                    if (voicer.RootPath == voicer2.RootPath)
                    {
                        newlyInstalled = false;
                    }

                    if (voicer.RootPath == voicer2.RootPath && voicer.Info.Version != voicer2.Info.Version)
                    {
                        installedVoicerUpdated = true;
                        updatedVoicerIndex = index;
                    }

                    ++index;
                }

                if (newlyInstalled)
                {
                    voicersToAdd.Add(voicer);
                }
                
                if (installedVoicerUpdated)
                {
                    VoicerMeta currentMetaBackup = Voicers[updatedVoicerIndex].CurrentVoicerMeta;
                    Voicer updatedVoicer = voicer;
                    int metaIndex = currentMetaBackup.SpeakerId;
                    if (metaIndex >= updatedVoicer.VoicerMetaCollection.Count)
                    {
                        metaIndex = 0;
                    }
                    updatedVoicer.CurrentVoicerMeta = updatedVoicer.VoicerMetaCollection[metaIndex];
                    updatedVoicer.RefreshNickAndStyle();
                    
                    voicersToUpdate.Add(new VoicerUpdate(updatedVoicer, updatedVoicerIndex));

                }
                
            }


            foreach (var voicer in Voicers)
            {
                bool removed = true;
                foreach (var voicer2 in _voicers)
                {
                    if (voicer.RootPath == voicer2.RootPath)
                    {
                        removed = false;
                    }
                }

                if (removed)
                {
                    voicersToRemove.Add(voicer);
                }
            }

            foreach (var voicer in voicersToAdd)
            {
                Voicers.Add(voicer);
            }

            foreach (var voicer in voicersToRemove)
            {
                if (voicer == CurrentVoicer)
                {
                    Log.Information($"CurrentVoicer: \"{voicer}\" is removed. Set CurrentVoicer to the first voicer in the list.");
                    CurrentVoicer = Voicers[0];
                    _currentDefaultVoicerIndex = 0;
                    OnPropertyChanged(nameof(CurrentVoicer));
                    OnPropertyChanged(nameof(CurrentDefaultVoicerIndex));
                    
                }
                Voicers.Remove(voicer);
            }

            
            foreach (var voicer in voicersToUpdate)
            {
                Log.Information($"CurrentVoicer: \"{voicer.Voicer}\" is updated.");
                Voicers.RemoveAt(voicer.Index);
                Voicers.Insert(voicer.Index, voicer.Voicer);

            }
            
        }

        bool Undobackuped = false;
        CultureInfo lastCulture;
        public bool CultureChanged = false;
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
                
                lastCulture = new CultureInfo(CurrentVoicer.Info.LangCode);
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

                if (lastCulture != new CultureInfo(CurrentVoicer.Info.LangCode))
                {
                    CultureChanged = true;
                    v.OnVoicerCultureChanged(new CultureInfo(CurrentVoicer.Info.LangCode));
                }
                else
                {
                    CultureChanged = false;
                }
            }
        }
        

    }

    
}
