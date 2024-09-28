using Avalonia.Controls;
using Mirivoice.ViewModels;
using R3;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using VYaml.Annotations;
using VYaml.Serialization;

namespace Mirivoice.Mirivoice.Core.Managers
{

    [YamlObject]
    public partial class RecentFiles
    {
        public List<string> Files { get; set; } = new List<string>();

        [YamlConstructor]
        /// <summary>
        ///  To Update ui, we should call UpdateUI(this) from MainViewModel -- after AddRecentFile initialized
        /// </summary>
        public RecentFiles() {
            if (Files == null)
            {
                Files = new List<string>();
            }
            Validate();

        }


        [YamlIgnore]
        private const int MaxNum = 10;


        public void Validate()
        {
            Files = Files.Select(f =>
            {
                if ( f is not null && File.Exists(f) && f != string.Empty)
                {
                    return f;
                }

                return null;
            }).Where(f => f != null).ToList();
        }

        public void AddRecentFile(string filePath, MainViewModel v)
        {
            if (!Files.Contains(filePath))
            {
                Files.Add(filePath);
            }
            else
            {
                Files.Remove(filePath);
                Files.Add(filePath);
            }

            if (Files.Count == MaxNum + 1)
            {
                Files.RemoveAt(10);
            }
            Files.Reverse();
            Save();
            UpdateUI(v);
        }

        public void Save()
        {
            // save to file
            var yamlPath = MainManager.Instance.PathM.RecentFilesPath;
            WriteYaml(yamlPath);
        }
        
        public void UpdateUI(MainViewModel v)
        {
            List<MenuItem> newItems = new List<MenuItem>();

            foreach (var filePath in Files)
            {
                MenuItem newItem = new MenuItem
                {
                    Header = filePath,
                    Command = ReactiveCommand.Create(() => v.OpenProject(filePath))
                };

               newItems.Add(newItem);
            }
            v.RecentMenuCollection = new ObservableCollection<MenuItem>(newItems);
            v.OnPropertyChanged(nameof(v.RecentMenuCollection));
        }

        void WriteYaml(string yamlPath)
        {
            var utf8Yaml = YamlSerializer.SerializeToString(this);
            File.WriteAllText(yamlPath, utf8Yaml);
        }
    }
}
