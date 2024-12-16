using Avalonia.Controls;
using Mirivoice.ViewModels;
using Mirivoice.Views;

namespace Mirivoice.Commands
{
    public class SwapLineBoxCommand: ICommand
    {
        private MainViewModel v;
        private int i1;
        private int i2;

        private bool _canUndo = true;
        public bool CanUndo
        {
            get => _canUndo;
            set
            {
                _canUndo = value;
            }
        }
        public SwapLineBoxCommand(MainViewModel mainViewModel)
        {
            v = mainViewModel;
        }

        public void SetLineBoxesIdx(int i1, int i2)
        {
            this.i1 = i1;
            this.i2 = i2;
        }

        public void Execute(bool isRedoing)
        {
            LineBoxView t1 = v.LineBoxCollection[i1];
            v.LineBoxCollection.RemoveAt(i1);
            v.LineBoxCollection.Insert(i2, t1);

            RefreshLineNos();
        }

        public void UnExecute()
        {
            LineBoxView t1 = v.LineBoxCollection[i1];
            v.LineBoxCollection.RemoveAt(i1);
            v.LineBoxCollection.Insert(i2, t1);

            RefreshLineNos();
        }

        void RefreshLineNos()
        {
            foreach (LineBoxView lineBox in v.LineBoxCollection)
            {
                lineBox.viewModel.SetLineNo(v.LineBoxCollection.IndexOf(lineBox) + 1);
            }


        }
    }
}
