﻿using Mirivoice.Views;
using System;

namespace Mirivoice.Commands
{
    [Obsolete("[still working class]")]
    public class LockLineBoxCommand : ICommand
    {
        // Still implementing 
        LineBoxView l;

        public LockLineBoxCommand(LineBoxView lineBoxView)
        {
            l = lineBoxView;
        }


        public void Execute(bool isRedoing)
        {
            l.UnLock(false);
        }

        public void UnExecute()
        {
            l.UnLock(true);
        }


    }
}
