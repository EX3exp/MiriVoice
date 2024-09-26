using DynamicData;
using Mirivoice.Mirivoice.Core.Editor;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Views;
using System.Globalization;
using System.Reactive.Linq;

namespace Mirivoice.ViewModels
{
    public class SingleLineEditorViewModel : VoicerSelectingViewModelBase
    {
        public override MTextBoxEditor mTextBoxEditor { get; set; }
        public override void OnVoicerChanged(Voicer value) { }
        public override void OnVoicerCultureChanged(CultureInfo culture) { }

        public SingleLineEditorViewModel(string line=""): base(line, false)
        {

        }
    }


}
