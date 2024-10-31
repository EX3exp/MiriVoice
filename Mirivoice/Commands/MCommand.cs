using System;

namespace Mirivoice.Commands
{
    [Obsolete("This class will be obsolete, use ICommand instead")]
    public class MCommand : ICommand
    {
        private readonly MReceiver _receiver;

        public MCommand (MReceiver receiver)
        {
            _receiver = receiver;
        }

        public void Execute(bool isRedoing)
        {
            _receiver.DoAction();
        }

        public void UnExecute()
        {
            _receiver.UndoAction();
        }

    }
}

