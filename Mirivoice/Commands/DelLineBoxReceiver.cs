using Avalonia.Controls;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using System;
using System.Collections.ObjectModel;

namespace Mirivoice.Commands
{
    public class DelLineBoxReceiver : MReceiver
    {
        private MainViewModel v;
        private LineBoxView l;

        private LineBoxView lastLineBox;
        private int LineBoxIndexLastDeleted;
        private ItemsControl control;
        private ObservableCollection<MResult> lastResults;



        private SingleLineEditorView lastEditor;
        public DelLineBoxReceiver(MainViewModel mainViewModel, LineBoxView l)
        {
            this.l = l;
            v = mainViewModel;
        }

        public override void DoAction()
        {
            lastLineBox = l; // backup
            LineBoxIndexLastDeleted = Int32.Parse(l.viewModel.LineNo) - 1;
            control = l.Parent as ItemsControl;
            int RemoveIndex = Int32.Parse(l.viewModel.LineNo) - 1;
            RefreshLineNos(control);


            

            v.LineBoxCollection.RemoveAt(RemoveIndex);

            if (l.viewModel.IsSelected)
            {
                lastEditor = v.CurrentSingleLineEditor;
                v.CurrentSingleLineEditor = null;
                lastResults = new ObservableCollection<MResult>(l.MResultsCollection);
                v.MResultsCollection.Clear();
            }

            RefreshLineNos(control);
            v.OnPropertyChanged(nameof(v.CurrentSingleLineEditor));

        }

        public override void UndoAction()
        {
            
            v.LineBoxCollection.Insert(LineBoxIndexLastDeleted, lastLineBox);
            RefreshLineNos(control);
            if (lastEditor != null)
            {
                v.CurrentSingleLineEditor = lastEditor;
                v.OnPropertyChanged(nameof(v.CurrentSingleLineEditor));
                l.MResultsCollection = lastResults;
                v.MResultsCollection = lastResults;
                v.OnPropertyChanged(nameof(v.MResultsCollection));
            }
        }

        void RefreshLineNos(ItemsControl i)
        {
            int index = 0;
            foreach (LineBoxView lineBox in i.Items)
            {
                lineBox.viewModel.SetLineNo(index + 1);
                ++index;
            }


        }

       

    }
}
