﻿using Avalonia.Media;
using Avalonia.Platform;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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