using Mirivoice.Mirivoice.Core.Editor;
using Mirivoice.Mirivoice.Core.Format;
using System.Globalization;

namespace Mirivoice.ViewModels
{
    // Base class for view models that need to select a voicer.
    public abstract class VoicerSelectingViewModelBase: ViewModelBase
    {
        

        

        public int _currentDefaultVoicerIndex;
        public bool NotProcessingSetDefVoicerCommand = false;
        protected int lastDefaultVoicerIndex;


        public string _currentScript;
        public bool NotProcessingSetScriptCommand = false;
        protected string lastScript;
        public virtual VoicerSelector voicerSelector { get; set; }
        public virtual MTextBoxEditor mTextBoxEditor { get; set; }
        public VoicerSelectingViewModelBase(bool useVoicerSelector=false)
        {
            if (useVoicerSelector)
            {
                //Log.Debug("VoicerSelectingViewModelBase Constructor -- using voicerSelector");
                voicerSelector = new VoicerSelector(this, ref _currentDefaultVoicerIndex, ref lastDefaultVoicerIndex);
                voicerSelector.CurrentVoicerMetaCollection =
                    voicerSelector.Voicers[voicerSelector.CurrentDefaultVoicerIndex]
                    .VoicerMetaCollection;
            }
            else
            {
                //Log.Debug("VoicerSelectingViewModelBase Constructor -- not using voicerSelector");
                
            }
            

            mTextBoxEditor = new MTextBoxEditor(this, ref _currentScript, ref lastScript);
            mTextBoxEditor.CurrentScript = string.Empty; // save undo history
        }

        public VoicerSelectingViewModelBase(int voicerIndex, bool useVoicerSelector = false)
        {
            if (useVoicerSelector)
            {
                _currentDefaultVoicerIndex = voicerIndex;
                //Log.Debug("VoicerSelectingViewModelBase Constructor -- using voicerSelector");
                voicerSelector = new VoicerSelector(this, ref _currentDefaultVoicerIndex, ref lastDefaultVoicerIndex);
                voicerSelector.CurrentVoicerMetaCollection =
                    voicerSelector.Voicers[voicerSelector.CurrentDefaultVoicerIndex]
                    .VoicerMetaCollection;
            }
            else
            {
                //Log.Debug("VoicerSelectingViewModelBase Constructor -- not using voicerSelector");

            }


            mTextBoxEditor = new MTextBoxEditor(this, ref _currentScript, ref lastScript);
            mTextBoxEditor.CurrentScript = string.Empty; // save undo history
        }
        public abstract void OnVoicerChanged(Voicer value);
        public abstract void OnVoicerCultureChanged(CultureInfo culture);

        public VoicerSelectingViewModelBase(string initText, bool saveUndoAtInit, bool useVoicerSelector = false)
        {
            if (useVoicerSelector)
            {
                //Log.Debug("VoicerSelectingViewModelBase Constructor -- using voicerSelector");
                voicerSelector = new VoicerSelector(this, ref _currentDefaultVoicerIndex, ref lastDefaultVoicerIndex);
                voicerSelector.CurrentVoicerMetaCollection =
                    voicerSelector.Voicers[voicerSelector.CurrentDefaultVoicerIndex]
                    .VoicerMetaCollection;
            }
            else
            {
                //Log.Debug("VoicerSelectingViewModelBase Constructor -- not using voicerSelector");

            }

            _currentScript = initText;
            lastScript = initText;
            //Log.Debug($"VoicerSelectingViewModelBase Constructor -- using MTextBoxEditor {initText}, {_currentScript}");
            mTextBoxEditor = new MTextBoxEditor(this, ref _currentScript, ref lastScript);
        }

    }
}
