﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using DynamicData;
using Mirivoice;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using Serilog;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Mirivoice.Mirivoice.Core.Utils
{

    public class VoicerInstaller
    {
        const string kVoicerYaml = "voicer.yaml";

        private string basePath;
        private readonly Encoding archiveEncoding;
        private readonly Encoding textEncoding;
        private readonly MainViewModel v;

        public VoicerInstaller(MainViewModel v)
        {
            this.basePath = MainManager.Instance.PathM.VoicerPath;
            this.v = v;
            // make sure we use UTF-8 for all text files
            this.archiveEncoding = Encoding.UTF8;
            this.textEncoding = Encoding.UTF8;
        }
        string MakeDirectoryUnique(string path)
        {
            if (!Directory.Exists(path))
            {
                return path;
            }
            int i = 1;
            string newPath = path;
            while (Directory.Exists(newPath))
            {
                newPath = $"{path}({i})";
                ++i;
            }
            return newPath;
        }

        public async void InstallVoicers(string[] paths)
        {
            Log.Information($"Installing {paths.Length} Voicers");
            foreach (string p in paths)
            {
                Log.Information($"Installing voicer from {p}");
                // Currently it puts suffix to the directory name if it already exists, but if the bug in VoicerSelector.cs is fixed, it should be changed to overwrite the existing directory(to update Voicer).
                string finalPath = MakeDirectoryUnique(p);
                var result = await v.ShowTaskWindow("menu.tools.voicerinstall", "menu.tools.voicerinstall", Install(finalPath), "menu.tools.voicerinstall.process", "menu.tools.voicerinstall.success", "menu.tools.voicerinstall.failed");

                string dirName = System.IO.Path.GetFileNameWithoutExtension(finalPath);
                string extPath = System.IO.Path.Combine(basePath, dirName);
                if (! MainManager.Instance.VoicerM.ValidVoicerDirs.Contains(extPath))
                {
                    MainManager.Instance.VoicerM.ValidVoicerDirs.Add(extPath);
                    v.voicerSelector.Voicers.Add(MainManager.Instance.VoicerM.FindLastInstalledVoicer());
                }
                
                Log.Information($"Voicer installed.");
            }

            Log.Information($"Refresh Voicers");

            foreach (LineBoxView l in v.LineBoxCollection)
            {
                l.viewModel.voicerSelector.UpdateVoicerCollection();
            }
            Log.Information($"Voicers refreshed.");
        }
        public async Task<bool> Install(string path)
        {
            
            Log.Information($"Installing voicer from {path}");
            var readerOptions = new SharpCompress.Readers.ReaderOptions
            {
                ArchiveEncoding = new ArchiveEncoding
                {
                    Forced = archiveEncoding,
                }
            };
            var extractionOptions = new ExtractionOptions
            {
                Overwrite = true,
            };
            string extPath;
            using (var archive = ArchiveFactory.Open(path, readerOptions))
            {
                var touches = new List<string>();
                int total = archive.Entries.Count();
                int count = 0;
                bool hasVoicerYaml = archive.Entries.Any(e => System.IO.Path.GetFileName(e.Key) == kVoicerYaml);
                if (!hasVoicerYaml)
                {
                    await v.ShowConfirmWindow("menu.tools.voicerinstall.error");
                    Log.Error("Voicer archive does not contain voicer.yaml.");
                    return false;
                }

                string dirName = System.IO.Path.GetFileNameWithoutExtension(path);
                extPath = System.IO.Path.Combine(basePath, dirName);
                Directory.CreateDirectory(extPath);
                foreach (var entry in archive.Entries)
                {
                    if (entry.Key.Contains(".."))
                    {
                        // Prevent zipSlip attack
                        continue;
                    }
                    var filePath = System.IO.Path.Combine(extPath, entry.Key);
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
                    if (!entry.IsDirectory)
                    {
                        entry.WriteToFile(System.IO.Path.Combine(extPath, entry.Key), extractionOptions);
                    }
                }
                
            }
            
            
            
           

            return true;
        }


    }
}