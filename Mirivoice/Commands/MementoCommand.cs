using Serilog;

namespace Mirivoice.Commands
{
    public class MementoCommand<T> : ICommand
    {
        // Handles undo/redo in combobox, textbox, etc.
        // Similar to Memento Design pattern, It bases on Command Pattern.
        private readonly MOriginator<T> _originator;

        public T changedValue;
        private bool redoBlocked;

        public MementoCommand(MOriginator<T> originator)
        {
            _originator = originator;
            redoBlocked = true;
            
        }

        public void Execute(bool isRedoing)
        {
            //Log.Debug($"--- Execute");
            if (!isRedoing)
            {
                _originator.ClearRedoMemento();
                //_originator.PrintMemento();
                
                //Log.Debug($"-- exec");
            }
            else
            {
                if (!_originator.CanRedo())
                {
                    //Log.Debug($"-- Redo blocked");
                    return;
                }
                //Log.Debug($"-- Redoing");
                _originator.RestoreFromRedoMemento();
                //_originator.PrintMemento();
            }
        }

        public void Backup(T backupobj)
        {
            _originator.Backup(backupobj);
        }

        public void BackupRedo(T backupobj)
        {
            _originator.BackupRedo(backupobj);
        }


        public void UnExecute()
        {
            //Log.Debug("--- UnExecute");
            if (_originator.CanUndo())
            {
                _originator.RestoreFromUndoMemento();
                
                
            }
            else
            {
                //Log.Debug("--- Undo blocked");
            }
            //_originator.PrintMemento();
        }
    }
    }
