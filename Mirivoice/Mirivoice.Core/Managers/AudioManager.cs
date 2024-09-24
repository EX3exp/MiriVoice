using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using NAudio.Wave;
using Serilog;

namespace Mirivoice.Mirivoice.Core.Managers
{
    

    public class AudioManager
    {
        private IWavePlayer _waveOut;
        private List<AudioFileReader> _audioReaders;
        private int _currentFileIndex;
        private LineBoxView _;
        private readonly MainViewModel v;
        public AudioManager(MainViewModel v)
        {
            this.v = v;
            _waveOut = new WaveOutEvent();
            _waveOut.PlaybackStopped += OnPlaybackStopped;  // called when playback is stopped
            _audioReaders = new List<AudioFileReader>();
            _currentFileIndex = 0;
        }

        public string SaveToCache(byte[] audioData)
        {
            string cacheFileName = $"{Guid.NewGuid()}.wav";
            string cacheFilePath = Path.Combine(MainManager.Instance.PathM.CachePath, cacheFileName);
            File.WriteAllBytes(cacheFilePath, audioData);
            return cacheFilePath;
        }

        public string GetUniqueCachePath()
        {
            string cacheFileName = $"{Guid.NewGuid()}.wav";
            string cacheFilePath = Path.Combine(MainManager.Instance.PathM.CachePath, cacheFileName);
            Log.Debug($"Unique cache file path: {cacheFilePath}");
            return cacheFilePath;
        }

        string SetSuffixToUnique(string filepath, int suffix)
        {
            if (File.Exists(filepath))
            {
                string dirPath = Path.GetDirectoryName(filepath);
                string changedPath = Path.Combine(dirPath, Path.GetFileNameWithoutExtension(filepath) + $"({suffix})" + Path.GetExtension(filepath));
                if (File.Exists(changedPath))
                {
                    return SetSuffixToUnique(filepath, suffix + 1); // recursive call
                }
                else return changedPath;
            }
            else return filepath;
        }

        List<string> GetAllCacheFiles()
        {
            return Directory.GetFiles(MainManager.Instance.PathM.CachePath, "*.wav").ToList();
        }
        private int SelectedBtnIndexBeforePlay;
        private double OffsetBeforePlay;
        private int offset = 0;
        private int currentLine;
        /// <summary>
        /// Note that startIndex is same as lineNo (starts from 1, not 0)
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="exportOnlyAndDoNotPlay"></param>
        /// <param name="exportPerTrack"></param>
        /// <param name="fileName"></param>
        /// <param name="DirPath"></param>
        public async void PlayAllCacheFiles(int startIndex, bool exportOnlyAndDoNotPlay=false, bool exportPerTrack=true, string fileName="", string DirPath="")
        {
            if ( _waveOut != null && _waveOut.PlaybackState == PlaybackState.Paused)
            {
                _waveOut.Play();
                return;
            }
            List<string> caches = new List<string>();

            int index = 0;
            v.SingleTextBoxEditorEnabled = false;
            v.CurrentEdit.IsEnabled = false;
            var tasks = new List<Task>();
            
            caches.Clear();
            for (int i = startIndex - 1; i < v.LineBoxCollection.Count; ++i)
            {
                caches.Add(v.LineBoxCollection[i].CurrentCacheName);
            }
            v.MainWindowGetInput = false;
            for (int i = 0; i < v.LineBoxCollection.Count; ++i)
            {
                LineBoxView l = v.LineBoxCollection[i];

                if (i < startIndex - 1)
                {
                    Log.Debug($"[Skipping Cache Generation] Line {l.viewModel.LineNo} is before the start index.");
                    continue;
                }

                if (i == startIndex - 1)
                {
                    l.v.StartProgress(0, v.LineBoxCollection.Count - startIndex + 1, "Inference");
                }

                Log.Debug($"[Generating Cache]");

                // Start inference and add to the task list
                tasks.Add(Task.Run(async () =>
                {
                    await l.StartInference();
                    
                    l.v.ProgressProgressbar(1);
                }));
            }

            // Await all tasks to complete
            await Task.WhenAll(tasks);

            // Finalize progress
            if (v.LineBoxCollection.Count - 1 >= startIndex - 1)
            {
                v.LineBoxCollection[v.LineBoxCollection.Count - 1].v.EndProgress();
            }

            if (exportOnlyAndDoNotPlay)
            {
                
                Log.Information("Exporting cache files.");
                if (exportPerTrack)
                {
                    // export per track
                    int no = 1;
                    foreach (string cacheName in caches)
                    {
                        string exportPath = Path.Combine(DirPath, $"{no}_{fileName}.wav");
                      
                        exportPath = SetSuffixToUnique(exportPath, 1);
                        Log.Debug($"Exporting {cacheName} to {exportPath}");
                        // resample to 48000kHz 
                        using (var reader = new AudioFileReader(cacheName))
                        {
                            using (var resampler = new MediaFoundationResampler(reader, new WaveFormat(48000, reader.WaveFormat.Channels)))
                            {
                                resampler.ResamplerQuality = 60;
                                
                                WaveFileWriter.CreateWaveFile(exportPath, resampler);
                            }
                        }
                        no++;
                    }
                    
                }
                else
                {
                    // export mixdown
                    string exportPath = Path.Combine(DirPath, $"{fileName}.wav"); 
                    exportPath = SetSuffixToUnique(exportPath, 1);
                    using (var outputWaveFile = new WaveFileWriter(exportPath, new WaveFormat(48000, 1))) 
                    {
                        foreach (string cacheName in caches)
                        {
                            using (var reader = new AudioFileReader(cacheName))
                            {
                                // resample to 48000kHz
                                using (var resampler = new MediaFoundationResampler(reader, new WaveFormat(48000, reader.WaveFormat.Channels)))
                                {
                                    resampler.ResamplerQuality = 60;
                                    byte[] buffer = new byte[8192];
                                    int read;
                                    while ((read = resampler
                                        .Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        outputWaveFile.Write(buffer, 0, read);
                                    }

                                }
                            }
                        }
                    }
                }
                v.MainWindowGetInput = true;
                return;
            }

            foreach (string cacheName in caches)
            {
                //Log.Debug($"[Playing Cache] {cacheName}");
                var reader = new AudioFileReader(cacheName);
                _audioReaders.Add(reader);

                

            }

            // Linebox height is 104, so we need to offset the viewer by 104 * (startIndex - 1)
            OffsetBeforePlay = 104 * (startIndex - 1);
            SelectedBtnIndexBeforePlay = startIndex - 1;
            v.LinesViewerOffset = new Avalonia.Vector(0, 104 * (startIndex - 1));
            currentLine = startIndex - 1;
            
            PlayNextFile();
        }

        private void PlayNextFile()
        {

            if (_currentFileIndex < _audioReaders.Count)
            {

                _waveOut.Init(_audioReaders[_currentFileIndex]);
                v.LineBoxCollection[currentLine].viewModel.IsSelected = true;
                
                _waveOut.Play();
                
                
            }
        }

        
        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (_audioReaders.Count == 0) // when stopped
            {
                v.isPlaying = false;
                if (_waveOut != null)
                {
                    _waveOut.Dispose();
                }
                v.LineBoxCollection[currentLine].viewModel.IsSelected = false;
                v.LineBoxCollection[SelectedBtnIndexBeforePlay].viewModel.IsSelected = true;
                v.LinesViewerOffset = new Avalonia.Vector(0, OffsetBeforePlay);
                v.MainWindowGetInput = true;
                v.StopButtonEnabled = false;
               
                return;
            }
                _audioReaders[_currentFileIndex].Dispose();
            v.LineBoxCollection[currentLine].viewModel.IsSelected = false;
            v.LinesViewerOffset = new Avalonia.Vector(0, v.LinesViewerOffset.Y + 104);

            currentLine++;
            _currentFileIndex++;

            if (_currentFileIndex < _audioReaders.Count)
            {
                PlayNextFile();  
            }
            else
            {
                Log.Information("All cache files have been played.");
                v.isPlaying = false;
                
                _waveOut.Dispose();
                v.LineBoxCollection[currentLine-1].viewModel.IsSelected = false;
                v.LineBoxCollection[SelectedBtnIndexBeforePlay].viewModel.IsSelected = true;
                v.LinesViewerOffset = new Avalonia.Vector(0, OffsetBeforePlay);
                v.MainWindowGetInput = true;
                v.StopButtonEnabled = false;
            }
        }

        public void PauseAudio()
        {
            if (_waveOut is not null && _waveOut.PlaybackState == PlaybackState.Playing)
            {
                _waveOut.Pause();
            }
            
        }

        public void StopAudio()
        {
            

            if (_waveOut is not null)
            {
                _waveOut.Pause();
                _currentFileIndex = 0;
                _audioReaders.Clear();
                _waveOut.Stop();                
                
            }
            
        }
        public void DeleteCacheFile(string cacheFilePath)
        {
            if (File.Exists(cacheFilePath))
            {
                File.Delete(cacheFilePath);
            }
        }

        public void ClearCache()
        {
            StopAudio();
            foreach (var file in GetAllCacheFiles())
            {
                
                File.Delete(file);
            }
        }
    }

}
