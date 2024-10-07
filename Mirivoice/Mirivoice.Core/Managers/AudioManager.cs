using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using NAudio.Wave;
using Serilog;
using NetCoreAudio;
using Avalonia.Controls;
using System.Security.Cryptography;

namespace Mirivoice.Mirivoice.Core.Managers
{


    public class AudioManager
    {
        private Player _player = new();

        private List<string> audioPaths;
        private int _currentFileIndex;

        private readonly MainViewModel v;
        private bool MainViewModelPlaying;
        public bool IsPlaying => _player.Playing;
        public AudioManager(MainViewModel v)
        {
            this.v = v;
            //Log.Debug($"player: {_player}");
            _player.PlaybackFinished += OnPlaybackStopped; // called when playback is stopped
            audioPaths = new List<string>();
            _currentFileIndex = 0;
            MainViewModelPlaying = false;
        }

        public string SaveToCache(byte[] audioData)
        {
            string cacheFilePath = GetUniqueCachePath();
            File.WriteAllBytes(cacheFilePath, audioData);
            return cacheFilePath;
        }

        public static string GetUniqueCachePath()
        {
            string uniquename;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Guid.NewGuid().ToByteArray());

                uniquename = $"{BitConverter.ToString(hash).Replace("-", "").Substring(0, 4)}.wav";
                // gets the first 4 characters of the hash
            }

            string cacheFilePath = Path.Combine(MainManager.Instance.PathM.CachePath, uniquename);
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
        public TimeSpan GetAudioDuration(string audioFilePath)
        {
            using (var reader = new AudioFileReader(audioFilePath))
            {
                return reader.TotalTime;
            }
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
        public async void PlayAllCacheFiles(int startIndex, bool exportOnlyAndDoNotPlay = false, bool exportPerTrack = true, string fileName = "", string DirPath = "", bool exportSrtInsteadOfAudio = false)
        {
            MainViewModelPlaying = true;
            if (_player != null && _player.Paused)
            {
                //Log.Debug("Resuming playback.");
                await _player.Resume();
                return;
            }
            List<string> caches = new List<string>();
            List<string> lines = new List<string>();
            List<string> voicerNames = new List<string>();

            int index = 0;
            v.SingleTextBoxEditorEnabled = false;
            v.CurrentEdit.IsEnabled = false;
            var tasks = new List<Task>();

            caches.Clear();
            lines.Clear();
            voicerNames.Clear();
            for (int i = startIndex - 1; i < v.LineBoxCollection.Count; ++i)
            {
                caches.Add(v.LineBoxCollection[i].CurrentCacheName);
                lines.Add(v.LineBoxCollection[i].viewModel.LineText);
                voicerNames.Add(v.LineBoxCollection[i].viewModel.voicerSelector.CurrentVoicer.Info.Name);
            }
            v.MainWindowGetInput = false;
            for (int i = 0; i < v.LineBoxCollection.Count; ++i)
            {
                LineBoxView l = v.LineBoxCollection[i];

                if (i < startIndex - 1)
                {
                    //Log.Debug($"[Skipping Cache Generation] Line {l.viewModel.LineNo} is before the start index.");
                    continue;
                }

                if (i == startIndex - 1)
                {
                    l.v.StartProgress(0, v.LineBoxCollection.Count - startIndex + 1, "Inference");
                }

                //Log.Debug($"[Generating Cache]");

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
                    v.MainWindowGetInput = true;
                    return;
                }
                else
                {
                    string exportPath;
                    // export srt  for mixdown 
                    if (exportSrtInsteadOfAudio)
                    {
                        exportPath = Path.Combine(DirPath, $"{fileName}.srt"); // for srt with line texts
                        string exportPathNamesSrt = Path.Combine(DirPath, $"{fileName}-voicer names.srt"); // for srt with voicer names
                        exportPath = SetSuffixToUnique(exportPath, 1);
                        exportPathNamesSrt = SetSuffixToUnique(exportPathNamesSrt, 1);

                        TimeSpan lastTs = TimeSpan.Zero;
                        StringBuilder sb1 = new StringBuilder();
                        StringBuilder sb2 = new StringBuilder();
                        for (int i = 0; i < caches.Count; ++i)
                        {
                            sb1.AppendLine((i + 1).ToString());
                            sb2.AppendLine((i + 1).ToString());
                            TimeSpan currTs = GetAudioDuration(caches[i]);

                            string lastTime = lastTs.ToString(@"hh\:mm\:ss\,fff");
                            string currTime = currTs.ToString(@"hh\:mm\:ss\,fff");
                            sb1.AppendLine($"{lastTime} --> {currTime}"); // hours:minutes:seconds,milliseconds (00:00:00,000)
                            sb2.AppendLine($"{lastTime} --> {currTime}");
                            sb1.AppendLine(lines[i]);
                            sb2.AppendLine(voicerNames[i]);
                            sb1.AppendLine();
                            sb2.AppendLine();



                            lastTs = currTs;
                        }
                        File.WriteAllText(exportPath, sb1.ToString());
                        File.WriteAllText(exportPathNamesSrt, sb2.ToString());
                        v.MainWindowGetInput = true;
                        return;
                    }

                    // export mixdown
                    exportPath = Path.Combine(DirPath, $"{fileName}.wav");
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
                //Log.Debug($"[Collecting Cache] {cacheName}");
                audioPaths.Add(cacheName);



            }

            // Linebox height is 104, so we need to offset the viewer by 104 * (startIndex - 1)
            OffsetBeforePlay = 104 * (startIndex - 1);
            SelectedBtnIndexBeforePlay = startIndex - 1;
            v.LinesViewerOffset = new Avalonia.Vector(0, 104 * (startIndex - 1));
            currentLine = startIndex - 1;

            PlayNextFile();
        }

        private async void PlayNextFile()
        {

            if (_currentFileIndex < audioPaths.Count)
            {
                Log.Information($"Playing cache file: {audioPaths[_currentFileIndex]}");
                //Log.Debug($"currentLine: {currentLine}");
                //Log.Debug($"SelectedBtnIndexBeforePlay: {SelectedBtnIndexBeforePlay}");
                //Log.Debug($"OffsetBeforePlay: {OffsetBeforePlay}");
                //Log.Debug($"player: {_player}");
                await _player.Play(audioPaths[_currentFileIndex]);

                v.LineBoxCollection[currentLine].viewModel.IsSelected = true;

            }
        }

        public async void PlayAudio(string cacheFilePath)
        {
            if (MainViewModelPlaying)
            {
                return;
            }
            MainViewModelPlaying = false;
            if (File.Exists(cacheFilePath))
            {
                Log.Debug($"Playing cache file: {cacheFilePath}");
                await _player.Play(cacheFilePath);
            }
            else
            {
                Log.Error($"Cache file not found: {cacheFilePath}");
            }
        }

        private void OnPlaybackStopped(object? sender, EventArgs e)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                Log.Information("Playback stopped.");
                if (audioPaths.Count == 0) // when stopped
                {
                    v.isPlaying = false;
                    if (!MainViewModelPlaying)
                    {
                        return;
                    }
                    v.LineBoxCollection[currentLine].viewModel.IsSelected = false;
                    v.LineBoxCollection[SelectedBtnIndexBeforePlay].viewModel.IsSelected = true;
                    v.LinesViewerOffset = new Avalonia.Vector(0, OffsetBeforePlay);
                    v.MainWindowGetInput = true;
                    v.StopButtonEnabled = false;

                    return;
                }


                v.LineBoxCollection[currentLine].viewModel.IsSelected = false;
                v.LinesViewerOffset = new Avalonia.Vector(0, v.LinesViewerOffset.Y + 104);

                currentLine++;
                _currentFileIndex++;

                if (_currentFileIndex < audioPaths.Count)
                {
                    PlayNextFile();
                }
                else
                {
                    Log.Information("All cache files have been played.");

                    v.isPlaying = false;

                    v.LineBoxCollection[currentLine - 1].viewModel.IsSelected = false;
                    v.LineBoxCollection[SelectedBtnIndexBeforePlay].viewModel.IsSelected = true;
                    v.LinesViewerOffset = new Avalonia.Vector(0, OffsetBeforePlay);
                    v.MainWindowGetInput = true;
                    v.StopButtonEnabled = false;
                    MainViewModelPlaying = false;
                }
            });
        }

        public void PauseAudio()
        {
            if (_player is not null && _player.Playing)
            {
                _player.Pause();
            }

        }

        public void StopAudio()
        {


            if (_player is not null)
            {
                _player.Stop();
                _currentFileIndex = 0;
                audioPaths.Clear();
                v.MainWindowGetInput = true;
                MainViewModelPlaying = false;
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
