using System;
using System.IO;
using VYaml.Annotations;
using VYaml.Serialization;

namespace Mirivoice.Mirivoice.Core.Format
{
    [YamlObject]
    public partial class UserSetting
    {
        public Version Version { get; set; } = new Version(2, 0);
        public string Langcode { get; set; } = null; // ui language
        public bool ClearCacheOnQuit { get; set; } = true;
        public bool UseBeta { get; set; } = true;
        public string SkipUpdate { get; set; } = "";
        public string PlaybackDevice { get; set; } = string.Empty;
        public int PlaybackDeviceNumber { get; set; }
        public int? PlaybackDeviceIndex { get; set; }
        private int _maxSessionCount = 4;

        public int MaxSessionCount
        {
            get => _maxSessionCount;
            set
            {
                // if value is less than 1, it might cause loop, so we set it to 1 as minimum
                _maxSessionCount = Math.Max(1, value);
                if (MainManager.Instance.SessionM != null)
                {
                    MainManager.Instance.SessionM.SetNewMaxSessionCount(_maxSessionCount); // update session pool
                }
            }
        }
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
