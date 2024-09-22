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

        List<string> GetAllCacheFiles()
        {
            return Directory.GetFiles(MainManager.Instance.PathM.CachePath, "*.wav").ToList();
        }
        private int SelectedBtnIndexBeforePlay;
        private double OffsetBeforePlay;
        private int offset = 0;
        private int currentLine;
        public async void PlayAllCacheFiles(int startIndex)
        {
            if ( _waveOut != null && _waveOut.PlaybackState == PlaybackState.Paused)
            {
                _waveOut.Play();
                return;
            }
            List<string> caches = new List<string>();
            ObservableCollection<LineBoxView> lineBoxViews = v.LineBoxCollection;
            int index = 0;
            v.SingleTextBoxEditorEnabled = false;
            v.CurrentEdit.IsEnabled = false;
            var tasks = new List<Task>();
            
            caches.Clear();
            for (int i = startIndex - 1; i < lineBoxViews.Count; ++i)
            {
                caches.Add(lineBoxViews[i].CurrentCacheName);
            }

            for (int i = 0; i < lineBoxViews.Count; ++i)
            {
                LineBoxView l = lineBoxViews[i];

                if (i < startIndex - 1)
                {
                    Log.Debug($"[Skipping Cache Generation] Line {l.viewModel.LineNo} is before the start index.");
                    continue;
                }

                if (i == startIndex - 1)
                {
                    l.v.StartProgress(0, lineBoxViews.Count - startIndex + 1, "Inference");
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
            if (lineBoxViews.Count - 1 >= startIndex - 1)
            {
                lineBoxViews[lineBoxViews.Count - 1].v.EndProgress();
            }

            
            foreach (string cacheName in caches)
            {
                Log.Debug($"[Playing Cache] {cacheName}");
                var reader = new AudioFileReader(cacheName);
                _audioReaders.Add(reader);

                

            }

            // Linebox height is 104, so we need to offset the viewer by 104 * (startIndex - 1)
            OffsetBeforePlay = 104 * (startIndex - 1);
            SelectedBtnIndexBeforePlay = startIndex - 1;
            v.LinesViewerOffset = new Avalonia.Vector(0, 104 * (startIndex - 1));
            currentLine = startIndex - 1;
            v.MainWindowGetInput = false;
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
            foreach (var file in GetAllCacheFiles())
            {
                File.Delete(file);
            }
        }
    }

}
