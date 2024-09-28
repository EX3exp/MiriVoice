using Mirivoice.Views;

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
