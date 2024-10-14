using Mirivoice.Mirivoice.Core.Editor;
using Mirivoice.Mirivoice.Core.Format;
using System.Globalization;

namespace Mirivoice.ViewModels
{
    public class SingleLineEditorViewModel : VoicerSelectingViewModelBase
    {
        public override MTextBoxEditor mTextBoxEditor { get; set; }
        public override void OnVoicerChanged(Voicer value) { }
        public override void OnVoicerCultureChanged(CultureInfo culture) { }
        public override void OnStyleChanged() { }
        public SingleLineEditorViewModel(string line=""): base(line, false)
        {

        }
    }


}
