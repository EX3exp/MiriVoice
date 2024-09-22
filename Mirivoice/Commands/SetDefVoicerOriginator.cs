using Mirivoice.ViewModels;
using Serilog;

namespace Mirivoice.Commands
{
    public class SetDefVoicerOriginator : MOriginator<int>
    {
        private VoicerSelectingViewModelBase v;
        private int index;
        public SetDefVoicerOriginator(VoicerSelectingViewModelBase v, ref int index) : base(ref index)
        {
            this.index = index;
            this.v = v;
        }

        public override void UpdateProperties()
        {
            //Log.Debug("[Updating Properties] -- {obj}", obj);
            v.NotProcessingSetDefVoicerCommand = true; // prevent recursion loop
            v.voicerSelector.CurrentDefaultVoicerIndex = obj;

        }
    }
}
