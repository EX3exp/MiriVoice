using Avalonia;
using Mirivoice.ViewModels;
using Serilog;

namespace Mirivoice.Commands
{
    public class TextBoxEditCommand : ICommand
    {
        private VoicerSelectingViewModelBase v;

        Memento<string> undoMem = new Memento<string>();
        Memento<string> redoMem = new Memento<string>();
        private bool _canUndo = true;
        public bool CanUndo
        {
            get => _canUndo;
            set
            {
                _canUndo = value;
            }
        }

        bool isFirstExec = true;
        public void Execute(bool isRedoing)
        {
            if (isRedoing)
            {
                Log.Debug($"Redo: {redoMem}");
                if (!redoMem.CanPop) { return; }

                v.mTextBoxEditor.DonotExecCommand = true;
                v.mTextBoxEditor.CurrentScript = redoMem.Pop();
                v.mTextBoxEditor.DonotExecCommand = false;

            }
            else
            {
                if (!isFirstExec)
                {
                    undoMem.Push(v.mTextBoxEditor.CurrentScript);
                }
                isFirstExec = false;
                Log.Debug($"Execute: undo: {undoMem}, redo: {redoMem}");
            }
        }

        public void UnExecute()
        {
            
            redoMem.Push(v.mTextBoxEditor.CurrentScript);
            if (!undoMem.CanPop) { return; }
            v.mTextBoxEditor.DonotExecCommand = true;
            v.mTextBoxEditor.CurrentScript = undoMem.Pop();
            v.mTextBoxEditor.DonotExecCommand = false;
            Log.Debug($"Undo: undo: {undoMem} redo: {redoMem}");
            

        }
    public TextBoxEditCommand(VoicerSelectingViewModelBase v)
        {
            this.v = v;
            undoMem.Push(v.mTextBoxEditor.CurrentScript);
        }
    }
}
