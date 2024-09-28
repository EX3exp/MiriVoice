using Mirivoice.Views;
using System;

namespace Mirivoice.Commands
{
    public class DuplicateLineBoxReceiver : MReceiver
    {
        private LineBoxView l;
        private int LineBoxIndexLastAdded;
        public DuplicateLineBoxReceiver(LineBoxView l)
        {
            this.l = l;
        }

        public override void DoAction()
        {

            var lineBox = new LineBoxView(l);
            int LineNoToBeAdded = Int32.Parse(l.viewModel.LineNo);

            lineBox.viewModel.SetLineNo(LineNoToBeAdded + 1);

            l.v.LineBoxCollection.Insert(LineNoToBeAdded, lineBox);
            LineBoxIndexLastAdded = LineNoToBeAdded;

            RefreshLineNos();

        }

        public override void UndoAction()
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
