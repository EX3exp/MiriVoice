using Avalonia.Controls;
using Mirivoice.ViewModels;
using Mirivoice.Views;

namespace Mirivoice.Commands
{
    public class AddLineBoxCommand : ICommand
    {
        private readonly MainViewModel v;
        private int LineBoxIndexLastAdded;
        private bool _canUndo = true;
        public bool CanUndo
        {
            get => _canUndo;
            set
            {
                _canUndo = value;
            }
        }
        public AddLineBoxCommand(MainViewModel mainViewModel)
        {
            v = mainViewModel;
        }

        UserControl lastCurrentEdit;
        SingleLineEditorView lastSingleLineEditor;
        public void Execute(bool isRedoing)
        {
            if (isRedoing)
            {
                v.CurrentEdit = lastCurrentEdit;
                v.CurrentSingleLineEditor = lastSingleLineEditor;

            }
            var lineBox = new LineBoxView(v);
            int LineNoToBeAdded = v.LineBoxCollection.Count + 1;

            lineBox.viewModel.SetLineNo(LineNoToBeAdded);
            lineBox.ShouldPhonemizeWhenSelected = true;

            v.LineBoxCollection.Add(lineBox);
            LineBoxIndexLastAdded = v.LineBoxCollection.Count - 1;
            lineBox.ScrollToEnd();
            lastCurrentEdit = v.CurrentEdit;
            lastSingleLineEditor = v.CurrentSingleLineEditor;
        }

        public void UnExecute()
        {
            if (LineBoxIndexLastAdded < 0)
                return;

            v.LineBoxCollection.RemoveAt(LineBoxIndexLastAdded);

            v.CurrentEdit = null;
            v.CurrentSingleLineEditor = null;
            LineBoxIndexLastAdded -= 1;

        }



    }
}
