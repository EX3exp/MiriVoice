using Avalonia.Platform;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirivoice.Mirivoice.Plugins.Builtin.Phonemizers.Utils
{
    public class CmuDict
    {
        public Dictionary<string, string> CMUDict;

        public CmuDict()
        {
            CMUDict = new Dictionary<string, string>();
            var uri = new Uri("avares://Mirivoice/Assets/Plugin.Datas/cmudict.txt");
            var assets = AssetLoader.Open(uri);

            using (var stream = assets)
            {
                using (var reader = new StreamReader(stream))
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split("  ", 2);
                        if (parts.Length == 2)
                        {
                            string phone = parts[1].ToLower().Trim();
                            foreach (string p in phone.Split())
                            {
                                // Remove stress markers, except ah0
                                if (p.EndsWith("0") && p != "ah0")
                                {
                                    phone = phone.Replace(p, p.Substring(0, p.Length - 1));
                                }
                                else if (p.EndsWith("1") || p.EndsWith("2"))
                                {
                                    phone = phone.Replace(p, p.Substring(0, p.Length - 1));
                                }
                            }
                                CMUDict[parts[0].Trim().ToLower()] = phone;
                        }
                            
                    }
                }
            }

            Log.Information("CMU Dict loaded");


        }
    }
}
