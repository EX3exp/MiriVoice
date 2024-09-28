using ReactiveUI;

namespace Mirivoice.ViewModels
{
    public class MessageWindowViewModel: ViewModelBase
    {
        string _message;

        public string Message
        {
            get => _message;
            set {
                this.RaiseAndSetIfChanged(ref _message, value);
                OnPropertyChanged(nameof(Message));
            }
        }
    }
}
