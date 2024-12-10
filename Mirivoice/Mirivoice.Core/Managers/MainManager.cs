using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Mirivoice.Core.Managers;
using Mirivoice.Mirivoice.Core.Utils;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using SharpCompress;
using VYaml.Serialization;
using SharpCompress.Archives;


namespace Mirivoice.Mirivoice.Core
{
    // 1 singleton only
    public class MainManager : SingletonBase<MainManager>
    {
        public Task? InitializationTask = null;

        public PathManager PathM = new PathManager();

        public CommandManager cmd = new CommandManager();

        public VoicerManager VoicerM = new VoicerManager();

        public IconManager IconM;// will be initialized in MainViewModel

        public AudioManager AudioM; // will be initialized in MainViewModel

        public int DefaultVoicerIndex = 0;
        public int DefaultMetaIndex = 0;

        public UserSetting Setting = new UserSetting();
        public RecentFiles Recent = new RecentFiles();


        public void Initialize()
        {
            Log.Information($"RootPath: {MainManager.Instance.PathM.RootPath}");
            Log.Information($"DataPath: {MainManager.Instance.PathM.DataPath}");
            Log.Information($"CachePath: {MainManager.Instance.PathM.CachePath}");
            Log.Information($"SettingsPath: {MainManager.Instance.PathM.SettingsPath}");
            Log.Information($"RecentFilesPath: {MainManager.Instance.PathM.RecentFilesPath}");
            Log.Information($"LogFilePath: {MainManager.Instance.PathM.LogFilePath}");
            Log.Information($"VoicerPath: {MainManager.Instance.PathM.VoicerPath}");
            Log.Information($"AssetsPath: {MainManager.Instance.PathM.AssetsPath}");

            CheckDirs();
            UpdateDefaultVoicers();
            LoadVoicerManager();
            LoadSetting();
            LoadRecentFiles();
            
            InitializationTask = Task.Run(() => {
                Log.Information("MainManager Initialize Start");
            });
            
        }

        public void LoadVoicerManager()
        {
            VoicerM.LoadVoicers(PathM);
            
        }
        public void LoadSetting()
        {
            if (File.Exists(MainManager.Instance.PathM.SettingsPath))
            {
                var yamlUtf8Bytes = System.Text.Encoding.UTF8.GetBytes(ReadTxtFile(MainManager.Instance.PathM.SettingsPath));
                MainManager.Instance.Setting = YamlSerializer.Deserialize<UserSetting>(yamlUtf8Bytes);
            }
            else
            {
                MainManager.Instance.Setting.Save();
            }
        }

        public void LoadRecentFiles()
        {
            if (File.Exists(MainManager.Instance.PathM.RecentFilesPath))
            {
                var yamlUtf8Bytes = System.Text.Encoding.UTF8.GetBytes(ReadTxtFile(MainManager.Instance.PathM.RecentFilesPath));
                MainManager.Instance.Recent = YamlSerializer.Deserialize<RecentFiles>(yamlUtf8Bytes);
            }
            else
            {
                MainManager.Instance.Recent.Save();
            }
        }
        
        private static void DeleteExtractedZip(string zipFilePath)
        {
            // deletes zip file and split files
            string baseFileName = Path.GetFileNameWithoutExtension(zipFilePath);
            string directory = Path.GetDirectoryName(zipFilePath);

            // delete zip file
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
                Console.WriteLine($"Deleted: {zipFilePath}");
            }

            // delete split files
            int index = 1;
            while (true)
            {
                string splitFilePath = Path.Combine(directory, $"{baseFileName}.z{index:D2}");
                if (File.Exists(splitFilePath))
                {
                    File.Delete(splitFilePath);
                    index++;
                }
                else
                {
                    break;
                }
            }
        }
        public void UpdateDefaultVoicers()
        {
            Log.Information("Updating default voicers.");
            string dirName = Path.Combine(MainManager.Instance.PathM.AssetsPath, "DefaultVoicers");
            if (Directory.Exists(dirName))
            {
                foreach (string file in Directory.GetFiles(dirName))
                {
                    if (Path.GetExtension(file).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            ExtractSplitZip(file, PathM.VoicerPath);
                            Log.Information($"Successfully extracted {file}.");
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Error extracting {file}: {ex.Message}");
                        }
                    }
                }
            }
        }



        public string ReadTxtFile(string txtPath)
        {
            using (StreamReader sr = new StreamReader(txtPath))
            {
                return (sr.ReadToEnd());
            }
        }
        public ObservableCollection<Voicer> GetVoicersCollectionNew()
        {
            return new ObservableCollection<Voicer>(VoicerM.LoadVoicers(PathM));
        }

        public static void CheckDirs()
        {
            // Check voicer path
            if (!System.IO.Directory.Exists(MainManager.Instance.PathM.VoicerPath))
            {
                System.IO.Directory.CreateDirectory(MainManager.Instance.PathM.VoicerPath);
            }

            if (!System.IO.Directory.Exists(MainManager.Instance.PathM.CachePath))
            {
                System.IO.Directory.CreateDirectory(MainManager.Instance.PathM.CachePath);
            }

            if (!File.Exists(MainManager.Instance.PathM.SettingsPath))
            {
                MainManager.Instance.Setting.Save();
            }
            if (!File.Exists(MainManager.Instance.PathM.RecentFilesPath))
            {
                MainManager.Instance.Setting.Save();
            }
        }

        public static void ExtractSplitZip(string zipFilePath, string extractPath)
        {
            using (var archive = ArchiveFactory.Open(zipFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        string entryPath = Path.Combine(extractPath, entry.Key);

                        string directoryPath = Path.GetDirectoryName(entryPath);
                        if (!string.IsNullOrEmpty(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        using (var stream = File.OpenWrite(entryPath))
                        {
                            entry.WriteTo(stream);
                        }
                    }
                }
            }

            try
            {
                DeleteExtractedZip(zipFilePath); 
            }
            catch (Exception ex)
            {
                Log.Error($"Error deleting extracted zip: {ex.Message}");
            }

        }

    }

    

}
