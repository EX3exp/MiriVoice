using Mirivoice.Engines;
using Mirivoice.Mirivoice.Core.Format;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VYaml.Serialization;

namespace Mirivoice.Mirivoice.Core.Managers
{
    public class VoicerManager
    {
        string[] VoicerDirs;
        public List<string> ValidVoicerDirs;

        public VoicerManager()
        {
            ValidVoicerDirs = new List<string>();   
        }

        public Voicer FindVoicerWithName(string name)
        {
            foreach (string voicerDir in ValidVoicerDirs)
            {
                VoicerInfo voicerInfo;
                string voicerYamlPath = Path.Combine(voicerDir, "voicer.yaml");
                var yamlUtf8Bytes = System.Text.Encoding.UTF8.GetBytes(ReadTxtFile(voicerYamlPath));
                voicerInfo = YamlSerializer.Deserialize<VoicerInfo>(yamlUtf8Bytes);
                if (voicerInfo.Name == name)
                {
                    Voicer voicer = new Voicer(voicerInfo);
                    voicer.SetRootPath(voicerDir);
                    return voicer;
                }
            }
            return null;
        }

        public Voicer FindLastInstalledVoicer()
        {
            VoicerInfo voicerInfo;
            string voicerDir = ValidVoicerDirs.Last();
            string voicerYamlPath = Path.Combine(voicerDir, "voicer.yaml");
            var yamlUtf8Bytes = System.Text.Encoding.UTF8.GetBytes(ReadTxtFile(voicerYamlPath));
            voicerInfo = YamlSerializer.Deserialize<VoicerInfo>(yamlUtf8Bytes);
            Voicer voicer = new Voicer(voicerInfo);
            voicer.SetRootPath(voicerDir);
            return voicer;
        }

        public int FindVoicerIndex(Voicer voicer)
        {
            Log.Information($"Valid dirs: {ValidVoicerDirs}");
          
            return ValidVoicerDirs.IndexOf(voicer.RootPath);
        }

        public Voicer? FindVoicerWithNameAndLangCodeAndUUID(string name, string langCode, string uuid)
        {
            // priority: langCode > name > uuid
            Voicer FindFirstMatchBy(Func<Voicer, bool> predicate, List<Voicer> voicers)
            {
                return voicers.FirstOrDefault(predicate);
            }

            // 1. read all voicers
            List<Voicer> allVoicers = ValidVoicerDirs
                .Select(ValidVoicerDir =>
                {
                    string voicerYamlPath = Path.Combine(ValidVoicerDir, "voicer.yaml");
                    string yamlContent = ReadTxtFile(voicerYamlPath);

                    if (string.IsNullOrEmpty(yamlContent))
                        return null;

                    var yamlUtf8Bytes = System.Text.Encoding.UTF8.GetBytes(yamlContent);
                    VoicerInfo voicerInfo = YamlSerializer.Deserialize<VoicerInfo>(yamlUtf8Bytes);

                    if (voicerInfo != null)
                    {
                        Voicer voicer = new Voicer(voicerInfo);
                        voicer.SetRootPath(ValidVoicerDir);
                        return voicer;
                    }

                    return null;
                })
                .Where(voicer => voicer != null)
                .ToList();

            if (allVoicers.Count == 0)
                return null;

            // 2. filter by langCode
            var voicerWithLang = FindFirstMatchBy(voicer => voicer.Info.LangCode == langCode, allVoicers);

            if (voicerWithLang != null)
                return voicerWithLang;

            // 3. filter by name
            var voicerWithName = FindFirstMatchBy(voicer => voicer.Info.Name == name, allVoicers);

            if (voicerWithName != null)
                return voicerWithName;

            // 4. filter by uuid
            var voicerWithUuid = FindFirstMatchBy(voicer => voicer.Info.uuid == uuid, allVoicers);

            return voicerWithUuid;
        }

        public List<Voicer> LoadVoicers (PathManager pathM)
        {
            VoicerDirs = Directory.GetDirectories(pathM.VoicerPath);
            
            List<Voicer> voicers = new List<Voicer>();
            
            foreach (string voicerDir in VoicerDirs)
            {
                VoicerInfo voicerInfo;
                string voicerYamlPath = Path.Combine(voicerDir, "voicer.yaml");
                
                
                
                if (File.Exists(voicerYamlPath))
                {
                    var yamlUtf8Bytes = System.Text.Encoding.UTF8.GetBytes(ReadTxtFile(voicerYamlPath));
                    voicerInfo = YamlSerializer.Deserialize<VoicerInfo>(yamlUtf8Bytes);


                    List<VoicerMeta> voicerMetas = new List<VoicerMeta>();
                    Voicer voicer = new Voicer(voicerInfo);
                    voicer.SetRootPath(voicerDir);
                    
                    
                    voicers.Add(voicer);
                    if (! ValidVoicerDirs.Contains(voicerDir))
                    {
                        ValidVoicerDirs.Add(voicerDir);
                    }
                    
                }
                else
                {
                    WriteYaml(voicerYamlPath, new VoicerInfo());
                }
            }

            if (voicers.Count == 0)
            {
                Log.Error("No Voicers Found");
            }
            else
            {
                Log.Information($"VoicerManager Loaded Voicers: {voicers.Count}");
            }
            
            return voicers;
        }

        

        public string GetVoicerDir(int voicerIndex)
        {
            return ValidVoicerDirs[voicerIndex];
        }


        string ReadTxtFile(string txtPath)
        {
            using (StreamReader sr = new StreamReader(txtPath))
            {
                return (sr.ReadToEnd());
            }    
        }

        void WriteYaml(string yamlPath, VoicerMeta obj)
        {
            var utf8Yaml = YamlSerializer.SerializeToString(obj);
            File.WriteAllText(yamlPath, utf8Yaml);
        }
        void WriteYaml(string yamlPath, VoicerInfo obj)
        {
            var utf8Yaml = YamlSerializer.SerializeToString(obj);
            File.WriteAllText(yamlPath, utf8Yaml);
        }


        void WriteYaml(string yamlPath, ConfigVITS2 obj)
        {
            var utf8Yaml = YamlSerializer.SerializeToString(obj);
            File.WriteAllText(yamlPath, utf8Yaml);
        }

        void WriteYaml(string yamlPath, Voicer obj)
        {
            var utf8Yaml = YamlSerializer.SerializeToString(obj);
            File.WriteAllText(yamlPath, utf8Yaml);
        }
    }
}
