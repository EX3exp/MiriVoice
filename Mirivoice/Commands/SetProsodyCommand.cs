using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;
using Serilog;

namespace Mirivoice.Commands
{
    public class SetProsodyCommand : ICommand
    {
        private MResult v;

        int undoMem;
        int redoMem;
        private bool _canUndo = true ;
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
                v.DonotExecCommand = true;
                v.Prosody = redoMem;
                v.DonotExecCommand = false;
            }
            else
            {
                

                if (!isFirstExec)
                {
                    undoMem = v.Prosody;
                }

                isFirstExec = false;
                
                Log.Debug($"Execute: undo: {undoMem}, redo: {redoMem}");
            }
        }

        public void UnExecute()
        {
            redoMem = v.Prosody;
            v.DonotExecCommand = true;
            v.Prosody = undoMem;
            v.DonotExecCommand = false;
            Log.Debug($"Undo: undo: {undoMem} redo: {redoMem}");


        }
        public SetProsodyCommand(MResult v)
        {
            this.v = v;
            undoMem = v.Prosody;
        }
    }

}
