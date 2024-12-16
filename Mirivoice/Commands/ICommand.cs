
namespace Mirivoice.Commands
{
    public interface ICommand
    {
        public void Execute(bool isRedoing);
        public bool CanUndo { get; set; }

        public void UnExecute();
    }
}
