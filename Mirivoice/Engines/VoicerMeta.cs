using System.ComponentModel;
using VYaml.Annotations;
namespace Mirivoice.Engines
{
    public enum VoicerMetaType
    {
        // name Voicer's type into 1~6 chars.
        // usally uses tts model's name.
        // If name is long than 6 chars, abbreviation would be needed.

        [Description ("VITS2")] VITS2 // VITS2
        
        //,UTAU // UTAU HANASU
        //, FASPCH // fastspeech
        //, TKTRN2 // tacotron2
    }

    [YamlObject]
    public partial class VoicerMeta
    {
        
        public string Style { get; set; } = "";
        public int SpeakerId { get; set; } = 0;
        public string Phrase { get; set; } = "";
        public string Description { get; set; } = "";
        
        


        

        public VoicerMeta()
        {
            
        }
        public override string ToString()
        {
            return $"{Style}";
        }



    }
}
