using Serilog;

namespace Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers.Utils
{
    /// <summary>
    /// Should call InitCMUDict() before using WordToArpabet
    /// </summary>
    public static class EnglishUSPhonemizerUtil
    {
        private static CmuDict cmuDict = new CmuDict();


        public static string WordToArpabet(string word)
        {
            // use the CMU Pronouncing Dictionary to convert words to ARPAbet
            string arpabetRes = cmuDict.CMUDict.TryGetValue(word.ToLower(), out string arpabet) ? arpabet.ToLower() : word;
            Log.Debug($"WordToArpabet: {word} -> {arpabetRes}");
            return arpabetRes;

        }
    }
}
