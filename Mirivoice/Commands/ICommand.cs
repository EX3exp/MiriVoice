
namespace Mirivoice.Commands
{
    public interface ICommand
    {
        public void Execute(bool isRedoing);

        public void UnExecute();
    }
}
