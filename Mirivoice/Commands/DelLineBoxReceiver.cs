using Avalonia.Controls;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mirivoice.Commands
{
    public class DelLineBoxReceiver : MReceiver
    {
        private MainViewModel v;
        private LineBoxView l;

        private LineBoxView lastLineBox;
        private int LineBoxIndexLastDeleted;
        private ItemsControl control;



        private UserControl lastEditor;
        public DelLineBoxReceiver(MainViewModel mainViewModel, LineBoxView l)
        {
            this.l = l;
            v = mainViewModel;
        }

        public override void DoAction()
        {
            lastLineBox = l; // backup
            LineBoxIndexLastDeleted = v.LineBoxCollection.IndexOf(l);
            control = l.Parent as ItemsControl;
            DeleteLineAndRefreshLineNos(control);

            

            v.LineBoxCollection.Remove(l);

            if (v.CurrentLineBox == l)
            {
                lastEditor = v.CurrentSingleLineEditor;
                v.CurrentSingleLineEditor = null;
                v.MResultsCollection.Clear();
            }
            v.OnPropertyChanged(nameof(v.CurrentSingleLineEditor));

        }

        public override void UndoAction()
        {
            UndoDeleteLineAndRefreshLineNos(control);
            v.LineBoxCollection.Insert(LineBoxIndexLastDeleted, lastLineBox);


        }

        void DeleteLineAndRefreshLineNos(ItemsControl i)
        {
            var _ = i.Items.ToList();
            _.Remove(l);

            foreach (LineBoxView lineBox in i.Items)
            {
                if (lineBox != l)
                {
                    lineBox.FindControl<TextBlock>("lineNumber").Text = (_.IndexOf(lineBox) + 1).ToString();
                }
            }


        }

        void UndoDeleteLineAndRefreshLineNos(ItemsControl i)
        {
            var _ = i.Items.ToList();
            _.Insert(LineBoxIndexLastDeleted, lastLineBox);

            foreach (LineBoxView lineBox in i.Items)
            {
                lineBox.FindControl<TextBlock>("lineNumber").Text = (_.IndexOf(lineBox) + 1).ToString();

            }


        }

    }
}
