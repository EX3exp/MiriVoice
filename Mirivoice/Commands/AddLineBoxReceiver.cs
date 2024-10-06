using Mirivoice.ViewModels;
using Mirivoice.Views;

namespace Mirivoice.Commands
{
    public class AddLineBoxReceiver : MReceiver
    {
        private readonly MainViewModel v;
        private int LineBoxIndexLastAdded;
        public AddLineBoxReceiver(MainViewModel mainViewModel)
        {
            v = mainViewModel;
        }

        public override void DoAction()
        {

            var lineBox = new LineBoxView(v); 
            int LineNoToBeAdded = v.LineBoxCollection.Count + 1;
        
            lineBox.viewModel.SetLineNo(LineNoToBeAdded);
            lineBox.ShouldPhonemizeWhenSelected = true;

            v.LineBoxCollection.Add(lineBox);
            LineBoxIndexLastAdded = v.LineBoxCollection.Count - 1;
            lineBox.ScrollToEnd();
        }

        public override void UndoAction()
        {
            if (LineBoxIndexLastAdded < 0)
                return;

            v.LineBoxCollection.RemoveAt(LineBoxIndexLastAdded);
            LineBoxIndexLastAdded -= 1;

        }


    }
}
