using Avalonia.Controls;
using Mirivoice.Mirivoice.Core;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mirivoice.Commands
{
    public class LockLineBoxReceiver : MReceiver
    {

        LineBoxView l;

        public LockLineBoxReceiver(LineBoxView lineBoxView)
        {
            l = lineBoxView;
        }


        public override void DoAction()
        {
            l.UnLock(false);
            
        }

        public override void UndoAction()
        {
            l.UnLock(true);
        }

    }
}
