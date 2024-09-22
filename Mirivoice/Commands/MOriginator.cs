using Serilog;
using System;

namespace Mirivoice.Commands
{
    public class MOriginator<T>
    {
        protected T obj;
        private T backup;
        private bool backupValid = false;

        private Memento<T> _undoMemento;
        private Memento<T> _redoMemento;

        public MOriginator(ref T obj)
        {
            //Log.Debug("MOriginator : {obj}", obj);
            this.obj = obj;
            this.backup = obj;
            backupValid = true;

            _redoMemento = new Memento<T>();
            _undoMemento = new Memento<T>();
        }

        
        public void PrintMemento()
        {
            Log.Debug("Undo Memento: {memento}", _undoMemento);
            Log.Debug("Redo Memento: {memento}", _redoMemento);
        }
       

        public bool CanUndo()
        {
            return _undoMemento.states.Count > 0;
        }

        public bool CanRedo()
        {
            return _redoMemento.states.Count > 0;
        }
        public void ClearRedoMemento()
        {
            Log.Debug("[Clearing redo memento]");
            _redoMemento.states.Clear();
        }
        public void RestoreFromUndoMemento()
        {
            Log.Debug($"Restoring from undo memento: {_undoMemento}, obj: {obj}");
            
            _redoMemento.states.Push(obj);
            obj = _undoMemento.states.Pop();
            UpdateProperties();
            Log.Debug("redo Memento: {memento}", _redoMemento);
        }

        public void Backup(T backup)
        {
            Log.Debug($"Backup: {backup}");
            this.backup = backup;
            _undoMemento.states.Push(backup);
        }

        public void BackupRedo(T backup)
        {
            Log.Debug($"Backup Redo: {backup}");
            _redoMemento.states.Push(backup);
        }
        
        public void RestoreFromRedoMemento()
        {
            Log.Debug($"Restoring from memento: {_redoMemento}");
            _undoMemento.states.Push(obj);
            obj = _redoMemento.states.Pop();
            UpdateProperties();
            
        }


        public virtual void UpdateProperties()
        {
            Log.Information("Updating Properties -- {obj}", obj);
            // update ui here
            throw new NotImplementedException();
        }

        
    }

}
