using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirivoice.ViewModels
{
    internal class AppUpdaterViewModel : ViewModelBase
    {
        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                this.RaiseAndSetIfChanged(ref _message, value);
                OnPropertyChanged(nameof(Message));
            }
        }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set
            {
                this.RaiseAndSetIfChanged(ref _progress, value);
                OnPropertyChanged(nameof(Progress));
            }
        }

        private bool _enableUpdateButton;
        public bool EnableUpdateButton
        {
            get => _enableUpdateButton;
            set
            {
                this.RaiseAndSetIfChanged(ref _enableUpdateButton, value);
                OnPropertyChanged(nameof(EnableUpdateButton));
            }
        }


    }
}
