using Avalonia.Controls;
using Mirivoice.ViewModels;
using Mirivoice.Views;

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
            int LineNoToBeAdded = l.v.LineBoxCollection.IndexOf(l) + 1;

            lineBox.FindControl<TextBlock>("lineNumber").Text = LineNoToBeAdded.ToString();
            
            
            l.v.LineBoxCollection.Insert(LineNoToBeAdded, lineBox);
            LineBoxIndexLastAdded = LineNoToBeAdded;

            

        }

        public override void UndoAction()
        {
            if (LineBoxIndexLastAdded < 0)
                return;

            l.v.LineBoxCollection.RemoveAt(LineBoxIndexLastAdded);
            LineBoxIndexLastAdded -= 1;

        }


    }
}
