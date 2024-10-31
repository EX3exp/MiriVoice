using Mirivoice.ViewModels;
using Mirivoice.Views;

namespace Mirivoice.Commands
{
    public class AddLineBoxCommand : ICommand
    {
        private readonly MainViewModel v;
        private int LineBoxIndexLastAdded;
        public AddLineBoxCommand(MainViewModel mainViewModel)
        {
            v = mainViewModel;
        }

        public void Execute(bool isRedoing)
        {
            var lineBox = new LineBoxView(v);
            int LineNoToBeAdded = v.LineBoxCollection.Count + 1;

            lineBox.viewModel.SetLineNo(LineNoToBeAdded);
            lineBox.ShouldPhonemizeWhenSelected = true;

            v.LineBoxCollection.Add(lineBox);
            LineBoxIndexLastAdded = v.LineBoxCollection.Count - 1;
            lineBox.ScrollToEnd();
        }

        public void UnExecute()
        {
            if (LineBoxIndexLastAdded < 0)
                return;

            v.LineBoxCollection.RemoveAt(LineBoxIndexLastAdded);
            LineBoxIndexLastAdded -= 1;

        }



    }
}
