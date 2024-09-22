using Avalonia.Controls;
using Mirivoice.ViewModels;
using Mirivoice.Views;

namespace Mirivoice.Commands
{
    public class SwapLineBoxReceiver: MReceiver
    {
        private MainViewModel v;
        private int i1;
        private int i2;


        public SwapLineBoxReceiver(MainViewModel mainViewModel)
        {
            v = mainViewModel;
        }

        public void SetLineBoxesIdx(int i1, int i2)
        {
            this.i1 = i1;
            this.i2 = i2;
        }

        public override void DoAction()
        {
            LineBoxView t1 = v.LineBoxCollection[i1];
            v.LineBoxCollection.RemoveAt(i1);
            v.LineBoxCollection.Insert(i2, t1);

            RefreshLineNos();

        }

        public override void UndoAction()
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
