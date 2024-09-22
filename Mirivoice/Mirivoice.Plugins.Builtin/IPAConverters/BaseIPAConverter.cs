namespace Mirivoice.Mirivoice.Plugins.Builtin.IPAConverters
{
    /// <summary>
    /// Base class for IPA converters. 
    /// </summary>
    public abstract class BaseIPAConverter
    {
        // Convert phonemes to IPA. Can be used in Mirivoice Engine itself, but can be used in TTS data preprocessing.

        /// <summary>
        ///  NOTE: Each IPA phonemes should be separated by TAB (\t).
        ///  e.g. "a	b	ʌ	d	ʑ	i	"
        /// </summary>  
        public abstract string ConvertToIPA(string phoneme, bool isFirstPhoneme);
        
    }
}
