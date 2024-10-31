using Avalonia.Controls;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using System;
using System.Collections.ObjectModel;

namespace Mirivoice.Commands
{
    public class DelLineBoxCommand : ICommand
    {
        private MainViewModel v;
        private LineBoxView l;

        private LineBoxView lastLineBox;
        private int LineBoxIndexLastDeleted;
        private ItemsControl control;
        private ObservableCollection<MResult> lastResults;



        private SingleLineEditorView lastEditor;

        public DelLineBoxCommand(MainViewModel mainViewModel, LineBoxView l)
        {
            this.l = l;
            v = mainViewModel;
        }

        public void Execute(bool isRedoing)
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

                if (v.CurrentEditIndex == 1)
                {
                    v.CurrentEdit = null;
                    v.OnPropertyChanged(nameof(v.CurrentEdit));
                }
            }

            RefreshLineNos(control);
            v.OnPropertyChanged(nameof(v.CurrentSingleLineEditor));
        }

        public void UnExecute()
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
                if (v.CurrentEditIndex == 1)
                {
                    v.CurrentEdit = l.ExpressionEditor;
                    v.OnPropertyChanged(nameof(v.CurrentEdit));
                }

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
