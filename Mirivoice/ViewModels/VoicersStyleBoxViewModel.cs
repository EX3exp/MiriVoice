using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
namespace Mirivoice.ViewModels
{
    public class VoicersStyleBoxViewModel : ViewModelBase
    {
        private bool _isDescOpen = false;
        public bool IsDescOpen
        {
            get => _isDescOpen;
            set
            {
                this.RaiseAndSetIfChanged(ref _isDescOpen, value);
                OnPropertyChanged(nameof(IsDescOpen));
            }
        }

        private bool _isDescSelected = false;
        public bool IsDescSelected
        {
            get => _isDescSelected;
            set
            {
                this.RaiseAndSetIfChanged(ref _isDescSelected, value);
                OnPropertyChanged(nameof(IsDescSelected));
            }
        }

        private string _descText;
        public string DescText
        {
            get => _descText;
            set
            {
                this.RaiseAndSetIfChanged(ref _descText, value);
                OnPropertyChanged(nameof(DescText));
            }
        }

        private string _styleName;
        public string StyleName
        {
            get => _styleName;
            set
            {
                this.RaiseAndSetIfChanged(ref _styleName, value);
                OnPropertyChanged(nameof(StyleName));
            }
        }
        public VoicersStyleBoxViewModel()
        {
        }

         
        public void OnDescButtonClick()
        {
            if (!IsDescSelected)
            {
                IsDescOpen = false;
            }
            else
            {
                IsDescOpen = true;
            }
        }
    }
}
