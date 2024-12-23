﻿using R3;
using Serilog;
using System;
using System.Collections.Generic;

namespace Mirivoice.Mirivoice.Plugins.Builtin.IPAConverters
{
    public class EnglishUSIPAConverter : BaseIPAConverter
    {
        // Many things are from https://github.com/wwesantos/arpabet-to-ipa/tree/master

        private readonly Dictionary<string, string> ArpaToIPA = new Dictionary<string, string>
        {
            { "AO", "ɔ" },
            { "AA", "ɑ" },
            { "IY", "i" },
            { "UW", "u" },
            { "EH", "e" }, // modern versions use 'e' instead of 'ɛ'
            { "IH", "ɪ" },
            { "UH", "ʊ" },
            { "AH", "ʌ" },
            { "AH0", "ə" },
            { "AE", "æ" },
            { "AX", "ə" },
            { "EY", "eɪ" },
            { "AY", "aɪ" },
            { "OW", "oʊ" },
            { "AW", "aʊ" },
            { "OY", "ɔɪ" },
            { "P", "p" },
            { "B", "b" },
            { "T", "t" },
            { "D", "d" },
            { "K", "k" },
            { "G", "g" },
            { "CH", "tʃ" },
            { "JH", "dʒ" },
            { "F", "f" },
            { "V", "v" },
            { "TH", "θ" },
            { "DH", "ð" },
            { "S", "s" },
            { "Z", "z" },
            { "SH", "ʃ" },
            { "ZH", "ʒ" },
            { "HH", "h" },
            { "M", "m" },
            { "N", "n" },
            { "NG", "ŋ" },
            { "L", "l" },
            { "R", "r" },
            { "ER", "ɜr" },
            { "AXR", "ər" },
            { "W", "w" },
            { "Y", "j" }
        };


        public override string ConvertToIPA(string phoneme, bool isFirstPhoneme)
        {
            List<string> IPA = new List<string>();
            foreach (string phone in phoneme.Split(" ", StringSplitOptions.RemoveEmptyEntries))
            {
                if (ArpaToIPA.ContainsKey(phone.ToUpper()))
                {
                    IPA.Add(ArpaToIPA[phone.ToUpper()]);
                }
                else
                {
                    IPA.Add(phone);
                }
            }
            string result = string.Join("", IPA);
            string res = string.Join("\t", result.ToCharArray());
            Log.Debug($"[IPA 변환] {phoneme} -> {res}");
            return res;
        }
    }
}
