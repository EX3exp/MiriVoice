using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;
using Serilog;
using System;

namespace Mirivoice.Commands
{
    public class SetProsodyOriginator : MOriginator<int>
    {
        private int index;

        private readonly MResult v;

        public SetProsodyOriginator(ref int index, MResult v) : base(ref index)
        {
            this.index = index;
            this.v = v;
        }


        public override void UpdateProperties()
        {
            Log.Debug("[Updating Properties] -- {obj}", obj);
            v.NotProcessingSetProsodyCommand = true; // prevent recursion loop
            v.Prosody = obj;
        }
    }
}
