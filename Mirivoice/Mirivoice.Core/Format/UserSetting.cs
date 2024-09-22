﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VYaml.Annotations;
using VYaml.Serialization;
using System.IO;

namespace Mirivoice.Mirivoice.Core.Format
{
    [YamlObject]
    public partial class UserSetting
    {
        public Version Version { get; set; } = new Version(1, 0);
        public string Langcode { get; set; } = "en-US"; // ui language
        public bool ClearCacheOnQuit { get; set; } = false;
        //public bool UseBeta { get; set; } = true;
        
        public void Save()
        {
            // save to file
            var yamlPath = MainManager.Instance.PathM.SettingsPath;
            WriteYaml(yamlPath);
        }

        void WriteYaml(string yamlPath)
        {
            var utf8Yaml = YamlSerializer.SerializeToString(this);
            File.WriteAllText(yamlPath, utf8Yaml);
        }
    }
}