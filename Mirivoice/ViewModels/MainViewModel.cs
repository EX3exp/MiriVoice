using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using global::Mirivoice.Views;
using Mirivoice.Commands;
using Mirivoice.Mirivoice.Core;
using Mirivoice.Mirivoice.Core.Editor;
using Mirivoice.Mirivoice.Core.Format;
using Mirivoice.Mirivoice.Core.Managers;
using Mirivoice.Mirivoice.Core.Utils;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VYaml.Serialization;
namespace Mirivoice.ViewModels
{
    public class MainViewModel : VoicerSelectingViewModelBase
    {
        MainWindow mainWindow;
        public static FilePickerFileType MiriVoiceProject { get; } = new("MiriVoice Project File")
        {
            Patterns = new[] { "*.mrp" },
            AppleUniformTypeIdentifiers = new[] { "com.ex3.mirivoice.project" },
            MimeTypes = new[] { "MiriVoiceProject/*" }
        };

        public static FilePickerFileType MiriVoiceExportAudio { get; } = new("Wav File")
        {
            Patterns = new[] { "*.wav" }
        };

        public static FilePickerFileType MiriVoiceExportSubRip { get; } = new("SubRip File")
        {
            Patterns = new[] { "*.srt" }
        };
        public static FilePickerFileType MiriVoiceVoicer { get; } = new("Zip File")
        {
            Patterns = new[] { "*.zip" }
        };

        Mrp initMrp;
        Version version = Assembly.GetEntryAssembly()?.GetName().Version;

        private bool _mainWindowGetInput = true;
        public bool MainWindowGetInput
        {
            get => _mainWindowGetInput;
            set
            {
                this.RaiseAndSetIfChanged(ref _mainWindowGetInput, value);
                OnPropertyChanged(nameof(MainWindowGetInput));
            }
        }
        private Vector _linesViewerOffset;
        public Vector LinesViewerOffset
        {
            get => _linesViewerOffset;
            set
            {
                if (_linesViewerOffset != value)
                {
                    this.RaiseAndSetIfChanged(ref _linesViewerOffset, value);
                    OnPropertyChanged(nameof(LinesViewerOffset)); // INotifyPropertyChanged 구현
                }
            }
        }
        private string _title;

        public string Title
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("MiriVoice v");
                sb.Append(version.ToString());
                sb.Append(" - ");
                sb.Append(CurrentProjectPath);
                if (MainManager.Instance.cmd != null)
                {
                    if (MainManager.Instance.cmd.IsNeedSave)
                    {
                        sb.Append("*");
                    }
                }

                return sb.ToString();
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _title, value);
                OnPropertyChanged(nameof(Title));
            }
        }
        public override VoicerSelector voicerSelector { get; set; }
        public override MTextBoxEditor mTextBoxEditor { get; set; }
        public SingleLineEditorView CurrentSingleLineEditor { get; set; }
        public UserControl CurrentEdit { get; set; }
        public bool SingleTextBoxEditorEnabled { get; set; } = false;

        private RecentFiles _recentFiles;
        public RecentFiles RecentFiles
        {
            get => _recentFiles;
            set
            {
                this.RaiseAndSetIfChanged(ref _recentFiles, value);
                OnPropertyChanged(nameof(RecentFiles));
            }
        }

        public ObservableCollection<MenuItem> RecentMenuCollection { get; set; } = new ObservableCollection<MenuItem>();
        public bool IsArcActive { get; set; } = false;

        public ObservableCollection<LineBoxView> LineBoxCollection { get; set; } = new ObservableCollection<LineBoxView>();

        public Mrp project
        {
            get => new Mrp(this);
            set { }
        }

        string _currentrojectPath;
        public string CurrentProjectPath
        {
            get => _currentrojectPath;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentrojectPath, value);
                OnPropertyChanged(nameof(CurrentProjectPath));
            }
        }
        public async void OnSaveButtonClick()
        {
            await Save();
        }

        public async void OnSaveAsButtonClick()
        {
            await SaveAs();
        }

        public void OnGithubButtonClick()
        {
            VoicersWindow.OpenUrl("https://github.com/EX3exp/MiriVoice");
        }
        public async Task SaveProject(bool forceSaveAs = false)
        {
            if (File.Exists(CurrentProjectPath) && !forceSaveAs)
            {
                project.Save(CurrentProjectPath);
            }
            else
            {
                
                var topLevel = TopLevel.GetTopLevel(mainWindow);
                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = (string)mainWindow.FindResource("app.saveproject"),
                    DefaultExtension = "mrp",
                    FileTypeChoices = new[] { MiriVoiceProject},
                    SuggestedFileName = (string)mainWindow.FindResource("app.defprojectname"),
                });

                if (file is not null)
                {
                    string path = file.Path.LocalPath;
                    CurrentProjectPath = path;
                    project.Save(path);
                    
                    RecentFiles.AddRecentFile(CurrentProjectPath, this);
                    OnPropertyChanged(nameof(RecentMenuCollection));
                }
            }
        }

        public async Task OnOpenButtonClick()
        {
            if (MainManager.Instance.cmd.IsNeedSave)
            {
                if (!await AskIfSaveAndContinue())
                {
                    return;
                }
            }
            var topLevel = TopLevel.GetTopLevel(mainWindow);

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = (string)mainWindow.FindResource("app.openproject"),
                AllowMultiple = false,
                FileTypeFilter = new[] { MiriVoiceProject }
            });

            if (files is not null && files.Count >= 1)
            {
                await OpenProject(files[0].Path.LocalPath);
            }
        }

        public async Task OpenProject(string path)
        {
            if (File.Exists(path))
            {
                Log.Information($"Opening project: {path}");
                var yamlUtf8Bytes = System.Text.Encoding.UTF8.GetBytes(MainManager.Instance.ReadTxtFile(path));
                Mrp mrp = YamlSerializer.Deserialize<Mrp>(yamlUtf8Bytes);
                try
                {
                    await mrp.Load(this);
                    project = mrp;
                    CurrentProjectPath = path;
                    RecentFiles.AddRecentFile(CurrentProjectPath, this);
                    OnPropertyChanged(nameof(RecentMenuCollection));
                    MainManager.Instance.cmd.ProjectOpened();
                    OnPropertyChanged(nameof(Title));
                }
                catch (Exception e)
                {
                    Log.Error($"[Failed to load project]{e.ToString}: {e.Message} \n>> traceback: \n\t{e.StackTrace}");
                    var res = await ShowConfirmWindow("app.openFailed");
                }

            }
            else
            {
                var res = await ShowConfirmWindow("menu.files.recent.openFailed");
            }
        }
        public ObservableCollection<MResult> MResultsCollection { get; set; } = new ObservableCollection<MResult>();

        private LineBoxView _currentLineBox;

        private bool _stopButtonEnabled = false;
        public bool StopButtonEnabled
        {
            get => _stopButtonEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _stopButtonEnabled, value);
                OnPropertyChanged(nameof(StopButtonEnabled));
            }
        }
        private string taskNameProgressBar; // for logging
        private bool _isVisibleProgressbar;
        private int _minTaskProgressbar;
        private int _maxTaskProgressbar;
        private int _valueProgressbar;

        private int _currentEditIndex;


        public int CurrentEditIndex
        {
            get => _currentEditIndex;
            set
            {
                if (value == -1)
                {
                    return;
                }

                this.RaiseAndSetIfChanged(ref _currentEditIndex, value);
                OnPropertyChanged(nameof(CurrentEditIndex));

                switch (_currentEditIndex)
                {
                    case 0:
                        if (CurrentLineBox is not null)
                        {
                            CurrentEdit = new PhonemeEditView();
                            
                        }
                        else
                        {
                            CurrentEdit = null;
                        }

                        this.OnPropertyChanged(nameof(CurrentEdit));
                        break;
                    case 1:
                        if (CurrentLineBox is not null)
                        {
                            CurrentEdit = CurrentLineBox.ExpressionEditor;
                            
                        }
                        else
                        {
                            CurrentEdit = null;
                        }
                        this.OnPropertyChanged(nameof(CurrentEdit));
                        break;
                    default:
                        CurrentEdit = null;
                        this.OnPropertyChanged(nameof(CurrentEdit));
                        break;
                }
            }
        }
        public bool IsVisibleProgressbar {
            get => _isVisibleProgressbar;
            set
            {
                this.RaiseAndSetIfChanged(ref _isVisibleProgressbar, value);
                OnPropertyChanged(nameof(IsVisibleProgressbar));
            }
        }

        public int MinTaskProgressbar
        {
            get => _minTaskProgressbar;
            set
            {
                this.RaiseAndSetIfChanged(ref _minTaskProgressbar, value);
                OnPropertyChanged(nameof(MinTaskProgressbar));
            }
        }
        public int MaxTaskProgressbar {
            get => _maxTaskProgressbar;
            set
            {
                this.RaiseAndSetIfChanged(ref _maxTaskProgressbar, value);
                OnPropertyChanged(nameof(MaxTaskProgressbar));
            }
        }
        public int ValueProgressbar {
            get => _valueProgressbar;
            set
            {
                this.RaiseAndSetIfChanged(ref _valueProgressbar, value);
                OnPropertyChanged(nameof(ValueProgressbar));
            }
        
        }
        public LineBoxView CurrentLineBox
        {
            get => _currentLineBox;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentLineBox, value);
                OnPropertyChanged(nameof(CurrentLineBox));
            }
        }

        public void Undo()
        {
            MainManager.Instance.cmd.Undo();
        }

        public void Redo()
        {
            MainManager.Instance.cmd.Redo();
        }
        
        public void ClearCache()
        {
            string cachePath = MainManager.Instance.PathM.CachePath;
            if (Directory.Exists(cachePath))
            {
                foreach (string file in Directory.GetFiles(cachePath))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Failed to delete cache file: {file} {e.Message}");
                    }
                }
            }
        }

        public void PlayAudio(string path)
        {
            MainManager.Instance.AudioM.PlayAudio(path);
            
        }

        public void StopAudio()
        {
            MainManager.Instance.AudioM.StopAudio();
            MainWindowGetInput = true;
        }
        public void OnAddButtonClick()
        {
            MainManager.Instance.DefaultVoicerIndex = voicerSelector.CurrentDefaultVoicerIndex;
            MainManager.Instance.DefaultMetaIndex = voicerSelector.Voicers[voicerSelector.CurrentDefaultVoicerIndex]
                .VoicerMetaCollection.IndexOf(voicerSelector.Voicers[voicerSelector.CurrentDefaultVoicerIndex].CurrentVoicerMeta);
            MainManager.Instance.cmd.ExecuteCommand(AddLineBoxCommand);
        }

        private bool _isPlaying;
        public bool isPlaying {
            get => _isPlaying;
            set
            {
                this.RaiseAndSetIfChanged(ref _isPlaying, value);
                if (value)
                {
                    SingleTextBoxEditorEnabled = false;
                    CurrentEdit.IsEnabled = false;
                }
                else
                {
                    // audio stop
                    
                    SingleTextBoxEditorEnabled = true;
                    CurrentEdit.IsEnabled = true;
                    MainManager.Instance.AudioM.PauseAudio();
                }
                OnPropertyChanged(nameof(isPlaying));
            }
        }
        public void OnPlayButtonClick()
        {
            
            StopButtonEnabled = true;
            if (!isPlaying)
            {
                int currentLineBoxIndex;
                if (CurrentLineBox == null)
                {
                    if (LineBoxCollection.Count == 0)
                    {
                        return;
                    }
                    currentLineBoxIndex = 1;
                }
                else
                {
                    currentLineBoxIndex = Int32.Parse(CurrentLineBox.viewModel.LineNo);
                }
                
                isPlaying = true;
                MainManager.Instance.AudioM.PlayAllCacheFiles(currentLineBoxIndex);
                Log.Information("Play Button Clicked");
            }
            else
            {
                isPlaying = false;
                
                Log.Information("Pause Button Clicked");
            }
        }
        IStorageFolder LastExportPath;
        public async void OnExportAudioPerLineClick()
        {
            if (LineBoxCollection.Count == 0)
            {
                return;
            }

            var topLevel = TopLevel.GetTopLevel(mainWindow);
            if (LastExportPath is null)
            {
                LastExportPath = topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Downloads).Result;
            }
            var directory = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = (string)mainWindow.FindResource("menu.files.export.perLine.desc"),
                AllowMultiple = false,
                SuggestedStartLocation = LastExportPath,
                SuggestedFileName = LastExportPath.Path.LocalPath

            });

            

            if (directory.Count > 0)
            {
                if (directory[0] is null)
                {
                    return;
                }
                string path = directory[0].Path.LocalPath;
                if (path == string.Empty) 
                {
                    path = LastExportPath.Path.LocalPath;
                }
                try
                {
                    Log.Information($"Exporting audio per line to {path}");
                    string filename = CurrentProjectPath.Split(System.IO.Path.PathSeparator).Last();
                    MainManager.Instance.AudioM.PlayAllCacheFiles(1, true, true, System.IO.Path.GetFileNameWithoutExtension(filename), path);
                    LastExportPath = directory[0];
                }
                catch (Exception e)
                {
                    Log.Error($"[Failed to export audio per line]{e.ToString}: {e.Message} \n>> traceback: \n\t{e.StackTrace}");
                    var res = await ShowConfirmWindow("menu.files.export.failed");
                }
            }
            MainWindowGetInput = true;

        }

        public async void OnExportAudioSelectedClick()
        {
            if (LineBoxCollection.Count == 0)
            {
                return;
            }
            if (CurrentLineBox is null)
            {
                var res = await ShowConfirmWindow("menu.files.export.selectedLine.noSelected");
                return;
            }
            var topLevel = TopLevel.GetTopLevel(mainWindow);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = (string)mainWindow.FindResource("menu.files.export.selectedLine.desc"),
                DefaultExtension = "wav",
                ShowOverwritePrompt = true,
                FileTypeChoices = new[] { MiriVoiceExportAudio },
                SuggestedFileName = System.IO.Path.GetFileNameWithoutExtension(CurrentProjectPath) + ".wav"
            });

            if (file is not null)
            {
                string path = file.Path.LocalPath;
                try
                {
                    MainManager.Instance.AudioM.PlayAllCacheFiles(1, true, false, System.IO.Path.GetFileNameWithoutExtension(path), System.IO.Path.GetDirectoryName(path), false, true);
                }
                catch (Exception e)
                {
                    Log.Error($"[Failed to export audio selected]{e.ToString}: {e.Message} \n>> traceback: \n\t{e.StackTrace}");
                    var res = await ShowConfirmWindow("menu.files.export.failed");
                }
            }
        }

        public async void OnExportAudioMergedClick()
        {
            if (LineBoxCollection.Count == 0)
            {
                return;
            }
            var topLevel = TopLevel.GetTopLevel(mainWindow);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = (string)mainWindow.FindResource("menu.files.export.merged.desc"),
                DefaultExtension = "wav",
                ShowOverwritePrompt = true,
                FileTypeChoices = new[] { MiriVoiceExportAudio },
                SuggestedFileName = System.IO.Path.GetFileNameWithoutExtension(CurrentProjectPath) + ".wav"
            });

            if (file is not null)
            {
                string path = file.Path.LocalPath;
                try
                {
                    MainManager.Instance.AudioM.PlayAllCacheFiles(1, true, false, System.IO.Path.GetFileNameWithoutExtension(path), System.IO.Path.GetDirectoryName(path) );
                }
                catch (Exception e)
                {
                    Log.Error($"[Failed to export audio merged]{e.ToString}: {e.Message} \n>> traceback: \n\t{e.StackTrace}");
                    var res = await ShowConfirmWindow("menu.files.export.failed");
                }
            }
        }

        public void OnGlobalSettingButtonClick()
        {
            Window window = new GlobalSettingWindow(mainWindow);

            window.Show();
        }
        public void OnStopButtonClick()
        {
            Log.Information("Stop Button Clicked");
            isPlaying = false;
            MainManager.Instance.AudioM.StopAudio();
            StopButtonEnabled = false;
        }

        public void OnAddBoxesButtonClick()
        {
            addLineBoxesReceiver.SetScript(mTextBoxEditor.CurrentScript);
            MainManager.Instance.DefaultVoicerIndex = voicerSelector.CurrentDefaultVoicerIndex;
            MainManager.Instance.DefaultMetaIndex = voicerSelector.Voicers[voicerSelector.CurrentDefaultVoicerIndex]
                .VoicerMetaCollection.IndexOf(voicerSelector.Voicers[voicerSelector.CurrentDefaultVoicerIndex].CurrentVoicerMeta);
            
            MainManager.Instance.cmd.ExecuteCommand(AddLineBoxesCommand);
        }

        public void OnUpdateCheckButtonClick()
        {
            var dialog = new AppUpdaterWindow(mainWindow);
            dialog.viewModel.CloseApplication =
                () => (Application.Current?.ApplicationLifetime as IControlledApplicationLifetime)?.Shutdown();
            dialog.ShowDialog(mainWindow);
        }

        public async void OnNewButtonClick()
        {

            if (MainManager.Instance.cmd.IsNeedSave)
            {
                if (!await AskIfSaveAndContinue())
                {
                    return;
                }
                project = initMrp;
                await project.Load(this);

                CurrentProjectPath = (string)mainWindow.FindResource("app.defprojectname");
                mTextBoxEditor.CurrentScript = "";

               
                
                CurrentEditIndex = 0;
    
                MainManager.Instance.cmd.ProjectOpened();
            }
            else
            {
                project = initMrp;
                await project.Load(this);

                CurrentProjectPath = (string)mainWindow.FindResource("app.defprojectname");
                mTextBoxEditor.CurrentScript = "";
                CurrentEditIndex = 0;
                MainWindowGetInput = true;
                MainManager.Instance.cmd.ProjectOpened();

            }


            LineBoxCollection.Clear();
            if (CurrentSingleLineEditor != null)
            {
                CurrentSingleLineEditor.viewModel.mTextBoxEditor.CurrentScript = string.Empty;
            }
            MResultsCollection.Clear();

        }


        public MainViewModel(MainWindow window) : base(true)
        {
            MainManager.Instance.AudioM = new AudioManager(this);
            MainManager.Instance.IconM = new IconManager();
            MainManager.Instance.cmd.SetMainViewModel(this);
            
            
            LoadRecentFIles();
            
            SetCommands();

            SingleTextBoxEditorEnabled = false;
            IsVisibleProgressbar = false;
            CurrentEditIndex = 0;
            mainWindow = window;

            mainWindow.Closing += WindowClosing;

            if (CurrentProjectPath == null || CurrentProjectPath == string.Empty)
            {
                CurrentProjectPath = (string)mainWindow.FindResource("app.defprojectname");
            }

        }

        bool forceClose = false;
        public async void WindowClosing(object? sender, WindowClosingEventArgs e)
        {
            if (forceClose)
            {
                if (MainManager.Instance.Setting.ClearCacheOnQuit)
                {
                    MainManager.Instance.AudioM.StopAudio();
                    try
                    {
                        Log.Information("Clearing cache...");
                        ClearCache();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to clear cache: {ex.Message}");
                    }
                    Log.Information("Cache cleared.");
                }
                return;
            }
            

            e.Cancel = true;
            if (MainManager.Instance.cmd.IsNeedSave)
            {
                if (!await AskIfSaveAndContinue())
                {
                    return;
                }
            }
            
            forceClose = true;
            mainWindow.Close();
        }
        public async Task Save()
        {
            await SaveProject(false);
            
        }

        async Task SaveAs()
        {
            await SaveProject(true);
        }


        private async Task<bool> AskIfSaveAndContinue()
        {

            var result = await MessageWindow.Show(mainWindow, (string)mainWindow.FindResource("app.beforeExit"), "", MessageWindow.MessageBoxButtons.YesNo);
            switch (result)
            {
                case MessageWindow.MessageBoxResult.Yes:
                    await SaveProject();
                    goto case MessageWindow.MessageBoxResult.No;
                case MessageWindow.MessageBoxResult.No:
                    return true; // Continue.
                default:
                    return false; // Cancel.
            }
        }

        public async Task<bool> AskIfSaveAndContinueForUpdate()
        {

            var result = await MessageWindow.Show(mainWindow, (string)mainWindow.FindResource("app.update.beforeExit"), "", MessageWindow.MessageBoxButtons.YesNo);
            switch (result)
            {
                case MessageWindow.MessageBoxResult.Yes:
                    await SaveProject();
                    goto case MessageWindow.MessageBoxResult.No;
                case MessageWindow.MessageBoxResult.No:
                    return true; // Continue.
                default:
                    return false; // Cancel.
            }
        }

        public async Task<bool> ShowConfirmWindow(string resourceId)
        {

            var result = await MessageWindow.Show(mainWindow, (string)mainWindow.FindResource(resourceId), "", MessageWindow.MessageBoxButtons.Ok);
            
            switch (result)
            {
                case MessageWindow.MessageBoxResult.Ok:
                    return true;
                default:
                    return false;
            }
        }

        public async Task<bool> ShowTaskWindow(string resourceIdText, string resourceIdTitle, Task<bool> task,
            string resourceIdProcessing, string resourceIdSuccess, string resourceIdFailed)
        {

            var result = await MessageWindow.Show(mainWindow, (string)mainWindow.FindResource(resourceIdText),
                (string)mainWindow.FindResource(resourceIdTitle), MessageWindow.MessageBoxButtons.OkWithProgress, task, (string)mainWindow.FindResource(resourceIdProcessing), (string)mainWindow.FindResource(resourceIdSuccess), (string)mainWindow.FindResource(resourceIdFailed)
                );

            switch (result)
            {
                case MessageWindow.MessageBoxResult.Ok:
                    return true;
                default:
                    return false;
            }
        }

        /*
        public async Task<bool> ShowErrorWindow(string errormsg)
        {

            var result = await MessageWindow.Show(mainWindow, errormsg, (string)mainWindow.FindResource("app.Error"), MessageWindow.MessageBoxButtons.Ok);

            switch (result)
            {
                case MessageWindow.MessageBoxResult.Ok:
                    return true;
                default:
                    return false;
            }
        }
        */

        public void StartProgress(int minTaskNum, int maxTaskNum, string taskName)
        {
            taskNameProgressBar = taskName;
            Log.Information($"{taskNameProgressBar} Started");
            
            MinTaskProgressbar = minTaskNum;
            MaxTaskProgressbar = maxTaskNum;
            IsVisibleProgressbar = true;
            ValueProgressbar = 0;
        }

        public void SetProgressValue(int value)
        {
            ValueProgressbar = value;
            //Log.Debug($"{value} / {MaxTaskProgressbar}");
        }

        public void ProgressProgressbar(int progressAmount)
        {
            int newValue;
            if (progressAmount > 0)
            {
                newValue = ValueProgressbar + progressAmount;
                if (newValue > MaxTaskProgressbar)
                {
                    newValue = MaxTaskProgressbar;
                }
                ValueProgressbar = newValue;
            }
            
        }

        public void EndProgress()
        {
            Log.Information($"{taskNameProgressBar} Completed");
            ValueProgressbar = MaxTaskProgressbar;
            IsVisibleProgressbar = false;
        }


        public void LoadRecentFIles()
        {
            if (!File.Exists(MainManager.Instance.PathM.RecentFilesPath))
            {
                RecentFiles = new RecentFiles();
                RecentFiles.Save();
                RecentFiles.Validate();
                OnPropertyChanged(nameof(RecentMenuCollection));

            }
            else
            {
                var yamlUtf8Bytes = System.Text.Encoding.UTF8.GetBytes(MainManager.Instance.ReadTxtFile(MainManager.Instance.PathM.RecentFilesPath));
                RecentFiles = YamlSerializer.Deserialize<RecentFiles>(yamlUtf8Bytes);
                RecentFiles.Validate();
                OnPropertyChanged(nameof(RecentMenuCollection));
            }
            RecentFiles.UpdateUI(this); // update UI
        }
        // Commands
        public MCommand AddLineBoxCommand { get; set; }
        AddLineBoxReceiver addLineBoxReceiver;

        public MCommand AddLineBoxesCommand { get; set; }
        AddLineBoxesReceiver addLineBoxesReceiver;


        void SetCommands()
        {
            addLineBoxReceiver = new AddLineBoxReceiver(this);
            AddLineBoxCommand = new MCommand(addLineBoxReceiver);

            addLineBoxesReceiver = new AddLineBoxesReceiver(this);
            AddLineBoxesCommand = new MCommand(addLineBoxesReceiver);



        }


        public void OnDataPreprocessButtonClick()
        {
            Window window = new DataPreprocessWindow(this, mainWindow);
            window.Show();
        }

        public void OnVoicersButtonClick()
        {
            Window window = new VoicersWindow(this, mainWindow);
            window.Show();
        }

        public async void OnExportSrtClick()
        {
            if (LineBoxCollection.Count == 0)
            {
                return;
            }
            var topLevel = TopLevel.GetTopLevel(mainWindow);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = (string)mainWindow.FindResource("menu.files.export.srt.desc"),
                DefaultExtension = "srt",
                ShowOverwritePrompt = true,
                FileTypeChoices = new[] { MiriVoiceExportSubRip },
                SuggestedFileName = System.IO.Path.GetFileNameWithoutExtension(CurrentProjectPath) + ".srt"
            });

            if (file is not null)
            {
                string path = file.Path.LocalPath;
                try
                {
                    MainManager.Instance.AudioM.PlayAllCacheFiles(1, true, false, System.IO.Path.GetFileNameWithoutExtension(path), System.IO.Path.GetDirectoryName(path), true);
                }
                catch (Exception e)
                {
                    Log.Error($"[Failed to export srt for merged audio]{e.ToString}: {e.Message} \n>> traceback: \n\t{e.StackTrace}");
                    var res = await ShowConfirmWindow("menu.files.export.failed");
                }
            }
        }
        public async void OnVoicerInstallButtonClick()
        {
            VoicerInstaller voicerInstaller = new VoicerInstaller(this);
            var topLevel = TopLevel.GetTopLevel(mainWindow);

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = (string)mainWindow.FindResource("menu.tools.voicerinstall.desc"),
                AllowMultiple = true,
                FileTypeFilter = new[] { MiriVoiceVoicer }
            });

            List<string> p_ = new List<string>();
            if (files is not null && files.Count >= 1)
            {
                
                foreach (var f in files)
                {
                    if (File.Exists(f.Path.LocalPath))
                    {
                        p_.Add(f.Path.LocalPath);
                    }
                    
                }
            }

            voicerInstaller.InstallVoicers(p_.ToArray());
        }

        public override void OnVoicerChanged(Voicer value) { }

        public override void OnVoicerCultureChanged(CultureInfo culture) { }

    }
}
