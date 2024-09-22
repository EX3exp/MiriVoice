using Mirivoice.ViewModels;
using Serilog;

namespace Mirivoice.Commands
{
    public class TextBoxEditOriginator : MOriginator<string>
    {
        private VoicerSelectingViewModelBase v;
        private string script;
        public TextBoxEditOriginator(VoicerSelectingViewModelBase v, ref string script) : base(ref script)
        {
            this.script = script;
            this.v = v;
        }

        public override void UpdateProperties()
        {

            //Log.Debug("[Updating Properties] -- {obj}", obj);
            v.NotProcessingSetScriptCommand = true; // prevent recursion loop
            v.mTextBoxEditor.CurrentScript = obj;

        }
    }
}
