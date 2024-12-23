﻿using Mirivoice.Commands;
using Serilog;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Mirivoice.ViewModels;
using System.Linq;

namespace Mirivoice.Mirivoice.Core.Managers
{
    public class CommandManager
    {
        private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();
        private MainViewModel v;
        public bool ChangedToDefVoicer = false;
        public bool IsNeedSave = true;


        public CommandManager()
        {
        }

        public bool IsAlreadyExecuted(ICommand target)
        {
            return _undoStack.Count > 0 && _undoStack.Peek() == target;
        }
        public void SetMainViewModel(MainViewModel v)
        {
            this.v = v;
            v.OnPropertyChanged(nameof(v.Title));
        }

        public void ProjectOpened()
        {
            ClearStacks();
            if (ChangedToDefVoicer)
            {
                IsNeedSave = true;
                ChangedToDefVoicer = false;
            }
            else
            {
                IsNeedSave = false;
            }
            
            v.OnPropertyChanged(nameof(v.Title));
        }

        public void ProjectSaved()
        {
            IsNeedSave = false;
            v.OnPropertyChanged(nameof(v.Title));
        }

        void ClearStacks()
        {
            //Log.Debug("======== Clear Stacks ========");
            _undoStack.Clear();
            _redoStack.Clear();
            IsNeedSave = false;
        }
        public void ExecuteCommand(ICommand command)
        {
            //Log.Debug("======== ExecuteCommand ========");
            command.Execute(false);
            _undoStack.Push(command);
            _redoStack.Clear();

            IsNeedSave = true;
            if (v != null)
            {
                v.OnPropertyChanged(nameof(v.Title));
            }
        }

        public void Undo()
        {
            //Log.Debug("======== Undo ========");

            
            if (_undoStack.Count > 0)
            {
                var command = _undoStack.Pop();
                while (!command.CanUndo)
                {
                    command = _undoStack.Pop(); // skip this undo and jump to previous one
                    if (_undoStack.Count == 0)
                    {
                        break;
                    }
                }
                command.UnExecute();
                _redoStack.Push(command);
                IsNeedSave = true;
            }

            
            if (v != null)
            {
                v.OnPropertyChanged(nameof(v.Title));
            }
        }

        public void Redo()
        {
            //Log.Debug("======== Redo ========");
            if (_redoStack.Count > 0)
            {
                var command = _redoStack.Pop();
                command.Execute(true);
                _undoStack.Push(command);
                IsNeedSave = true;
            }

            
            if (v != null)
            {
                v.OnPropertyChanged(nameof(v.Title));
            }
        }
    }
}
