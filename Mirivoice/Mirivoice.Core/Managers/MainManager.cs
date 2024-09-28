using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Mirivoice.Core.Managers;
using Mirivoice.Mirivoice.Core.Utils;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using VYaml.Serialization;

namespace Mirivoice.Mirivoice.Core
{
    // 1 singleton only
    public class MainManager : SingletonBase<MainManager>
    {
        public Task? InitializationTask = null;

        public PathManager PathM = new PathManager();

        public CommandManager cmd = new CommandManager();

        public VoicerManager VoicerM = new VoicerManager();

        public AudioManager AudioM; // will be initialized in MainViewModel

        public int DefaultVoicerIndex = 0;
        public int DefaultMetaIndex = 0;

        public UserSetting Setting = new UserSetting();
        public RecentFiles Recent = new RecentFiles();


        public void Initialize()
        {
            CheckDirs();
            LoadVoicerManager();
            LoadSetting();
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
            if (File.Exists(PathM.SettingsPath))
            {
                var yamlUtf8Bytes = System.Text.Encoding.UTF8.GetBytes(ReadTxtFile(PathM.SettingsPath));
                Setting = YamlSerializer.Deserialize<UserSetting>(yamlUtf8Bytes);
            }
            else
            {
                Setting.Save();
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



    }

    

}
