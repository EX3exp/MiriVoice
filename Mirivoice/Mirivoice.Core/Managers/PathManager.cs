using Mirivoice.Mirivoice.Core.Utils;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Mirivoice.Mirivoice.Core.Managers
{
    public class PathManager
    {
        public string RootPath { get; private set; }
        public string DataPath { get; private set; }
        public string CachePath { get; private set; }
        public string SettingsPath { get; private set; }
        
        public string RecentFilesPath { get; private set; }
        public bool HomePathIsAscii { get; private set; }
        public bool IsInstalled { get; private set; }

        public string LogFilePath => Path.Combine(DataPath, "Logs", "log.txt");
        public string VoicerPath => Path.Combine(RootPath, "Voicers");
        public string AssetsPath => Path.Combine(RootPath, "Assets");

        public PathManager()
        {
            RootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (!Directory.Exists(RootPath))
            {
                Directory.CreateDirectory(RootPath);
            }

            SettingsPath = Path.Combine(DataPath, "settings.yaml");
            RecentFilesPath = Path.Combine(DataPath, "recent_files.yaml");

            if (OS.IsMacOS())
            {
                string userHome = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                DataPath = Path.Combine(userHome, "Library", "Mirivoice");
                CachePath = Path.Combine(userHome, "Library", "Caches", "Mirivoice");
                HomePathIsAscii = true;
                try
                {
                    // Deletes old cache.
                    string oldCache = Path.Combine(DataPath, "Cache");
                    if (Directory.Exists(oldCache))
                    {
                        Directory.Delete(oldCache, true);
                    }
                }
                catch { }

            }
            else if (OS.IsLinux())
            {
                string userHome = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (string.IsNullOrEmpty(dataHome))
                {
                    dataHome = Path.Combine(userHome, ".local", "share");
                }
                DataPath = Path.Combine(dataHome, "Mirivoice");
                string cacheHome = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
                if (string.IsNullOrEmpty(cacheHome))
                {
                    cacheHome = Path.Combine(userHome, ".cache");
                }
                CachePath = Path.Combine(cacheHome, "Mirivoice");
                HomePathIsAscii = true;
            }
            else
            {
                string exePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                IsInstalled = File.Exists(Path.Combine(exePath, "installed.txt"));
                if (!IsInstalled)
                {
                    DataPath = exePath;
                }
                else
                {
                    string dataHome = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    DataPath = Path.Combine(dataHome, "MiriVoice");
                }
                CachePath = Path.Combine(DataPath, "Cache");
                HomePathIsAscii = true;
                var etor = StringInfo.GetTextElementEnumerator(DataPath);
                while (etor.MoveNext())
                {
                    string s = etor.GetTextElement();
                    if (s.Length != 1 || s[0] >= 128)
                    {
                        HomePathIsAscii = false;
                        break;
                    }
                }

            }
        }
        


    }
}
