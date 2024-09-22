using DynamicData;
using Mirivoice.Mirivoice.Core.Editor;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Views;
using System.Reactive.Linq;

namespace Mirivoice.ViewModels
{
    public class SingleLineEditorViewModel : VoicerSelectingViewModelBase
    {
        public override MTextBoxEditor mTextBoxEditor { get; set; }
        public override void OnVoicerChanged(Voicer value) { }


        public SingleLineEditorViewModel(string line=""): base(line, false)
        {

        }
    }


}
