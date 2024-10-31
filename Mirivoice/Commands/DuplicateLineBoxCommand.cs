using Mirivoice.Engines;
using Mirivoice.Mirivoice.Core;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Views;
using System;

namespace Mirivoice.Commands
{
    public class DuplicateLineBoxCommand : ICommand
    {
        private LineBoxView l;
        private int LineBoxIndexLastAdded;
        public DuplicateLineBoxCommand(LineBoxView l)
        {
            this.l = l;
        }

        public void Execute(bool isRedoing)
        {
            MLinePrototype mLinePrototype = new MLinePrototype(l);
            int LineNoToBeAdded = Int32.Parse(l.viewModel.LineNo);

            int spkid = l.viewModel.voicerSelector.CurrentVoicer.CurrentVoicerMeta.SpeakerId;
            int metaIndex = 0;
            foreach (VoicerMeta v in l.viewModel.voicerSelector.CurrentVoicer.VoicerMetaCollection)
            {
                if (v.SpeakerId == spkid)
                {
                    break;
                }
                metaIndex++;
            }
            var lineBox = new LineBoxView(mLinePrototype, l.v, LineNoToBeAdded + 1, l.viewModel.voicerSelector.CurrentDefaultVoicerIndex, metaIndex, true);


            l.v.LineBoxCollection.Insert(LineNoToBeAdded, lineBox);
            LineBoxIndexLastAdded = LineNoToBeAdded;

            RefreshLineNos();

        }
        public void UnExecute()
        {
            if (LineBoxIndexLastAdded < 0)
                return;

            l.v.LineBoxCollection.RemoveAt(LineBoxIndexLastAdded);
            LineBoxIndexLastAdded -= 1;
            RefreshLineNos();
        }

        void RefreshLineNos()
        {
            int index = 0;
            foreach (LineBoxView lineBox in l.v.LineBoxCollection)
            {
                lineBox.viewModel.SetLineNo(index + 1);
                ++index;
            }

        }
    }
}
