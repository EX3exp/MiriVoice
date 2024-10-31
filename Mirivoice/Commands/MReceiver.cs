using System;
using System.Collections.Generic;

namespace Mirivoice.Commands
{
    [Obsolete("This class will be obsolete, use ICommand instead")]
    public abstract class MReceiver
    {
        protected readonly List<object> _data = new List<object>();

        public virtual void DoAction()
        {
            //_data.Add(item);
            throw new NotImplementedException();
        }

        public virtual void UndoAction()
        {
            //_data.Remove(item);
            throw new NotImplementedException();
        }
    }
}
