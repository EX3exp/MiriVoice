using Avalonia.Controls;
using Avalonia.Media;
using Mirivoice.Mirivoice.Core;
using Mirivoice.Mirivoice.Core.Utils;
using NetSparkleUpdater;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.SignatureVerifiers;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Mirivoice.ViewModels
{
    public class AppUpdaterViewModel : ViewModelBase
    {
        class GithubReleaseAsset
        {
            public string name = string.Empty;
            public string browser_download_url = string.Empty;
        }
        class GithubRelease
        {
#pragma warning disable 0649
            public string html_url = string.Empty;
            public long id = long.MaxValue;
            public bool draft;
            public bool prerelease;
            public string name = string.Empty;
            public GithubReleaseAsset[] assets = new GithubReleaseAsset[0];
#pragma warning restore 0649
        }
        public string AppVersion => $"v{Assembly.GetEntryAssembly()?.GetName().Version}";

        private string _updateStatus;
        public string UpdaterStatus
        {
            get => _updateStatus;
            set
            {
                this.RaiseAndSetIfChanged(ref _updateStatus, value);
                OnPropertyChanged(nameof(UpdaterStatus));
            }
        }
        private bool _updateAvailable;
        public bool UpdateAvailable
        {
            get => _updateAvailable;
            set
            {
                this.RaiseAndSetIfChanged(ref _updateAvailable, value);
                OnPropertyChanged(nameof(UpdateAvailable));
            }
        }
        private FontWeight _updateButtonFontWeight;
        public FontWeight UpdateButtonFontWeight
        {
            get => _updateButtonFontWeight;
            set
            {
                this.RaiseAndSetIfChanged(ref _updateButtonFontWeight, value);
                OnPropertyChanged(nameof(UpdateButtonFontWeight));
            }
        }
        public Action? CloseApplication { get; set; }

        private SparkleUpdater? sparkle;
        private UpdateInfo? updateInfo;
        private bool updateAccepted;
        private Window mainWindow;

        public AppUpdaterViewModel(Window w)
        {
            UpdaterStatus = string.Empty;
            mainWindow = w;
            UpdateAvailable = false;
            UpdateButtonFontWeight = FontWeight.Normal;
            Init(w);
        }

        public static async Task<SparkleUpdater?> NewUpdaterAsync()
        {
            try
            {
                var release = await SelectRelease();
                if (release == null)
                {
                    Log.Error("No updatable release found.");
                    return null;
                }
                Log.Information($"Checking update at: {release.html_url}");
                var appcast = SelectAppcast(release);
                if (appcast == null)
                {
                    Log.Error("No updatable appcast found.");
                    return null;
                }
                Log.Information($"Checking appcast: {appcast.browser_download_url}");
                return new ZipUpdater(appcast.browser_download_url, new Ed25519Checker(SecurityMode.Unsafe))
                {
                    UIFactory = null,
                    CheckServerFileName = false,
                    RelaunchAfterUpdate = true,
                    RelaunchAfterUpdateCommandPrefix = OS.IsLinux() ? "./" : string.Empty,
                    AppCastHandler = new XMLAppCast()
                    {
                        AppCastFilter = new DowngradableFilter()
                    },
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to select appcast to update.");
                return null;
            }
        }

        static async Task<GithubRelease?> SelectRelease()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "Other");
            client.Timeout = TimeSpan.FromSeconds(30);
            using var resposne = await client.GetAsync("https://api.github.com/repos/EX3exp/MiriVoice/releases");
            resposne.EnsureSuccessStatusCode();
            string respBody = await resposne.Content.ReadAsStringAsync();
            List<GithubRelease>? releases = JsonConvert.DeserializeObject<List<GithubRelease>>(respBody);
            if (releases == null)
            {
                return null;
            }
            return releases
                .Where(r => !r.draft && r.prerelease == MainManager.Instance.Setting.UseBeta)
                .OrderByDescending(r => r.id)
                .FirstOrDefault();
        }

        static GithubReleaseAsset? SelectAppcast(GithubRelease release)
        {
            string suffix = MainManager.Instance.PathM.IsInstalled ? "-installer" : "";
            return release.assets
                .Where(a => a.name == $"appcast.{OS.GetUpdaterRid()}{suffix}.xml")
                .FirstOrDefault();
        }

        async void Init(Window mainWindow)
        {
            UpdaterStatus = (string)mainWindow.FindResource("updater.status.checking");
            sparkle = await NewUpdaterAsync();
            if (sparkle == null)
            {
                UpdaterStatus = (string)mainWindow.FindResource("updater.status.unknown");
                return;
            }
            updateInfo = await sparkle.CheckForUpdatesQuietly();
            if (updateInfo == null)
            {
                UpdaterStatus = (string)mainWindow.FindResource("updater.status.unknown");
                return;
            }
            switch (updateInfo.Status)
            {
                case UpdateStatus.UpdateAvailable:
                case UpdateStatus.UserSkipped:
                    UpdaterStatus = string.Format((string)mainWindow.FindResource("updater.status.available"), updateInfo.Updates[0].Version);
                    UpdateAvailable = true;
                    UpdateButtonFontWeight = FontWeight.Bold;
                    break;
                case UpdateStatus.UpdateNotAvailable:
                    UpdaterStatus = (string)mainWindow.FindResource("updater.status.notavailable");
                    break;
                case UpdateStatus.CouldNotDetermine:
                    UpdaterStatus = (string)mainWindow.FindResource("updater.status.unknown");
                    break;
            }
        }


        public async void OnUpdate()
        {
            if (sparkle == null || updateInfo == null || updateInfo.Updates.Count == 0)
            {
                return;
            }
            UpdateAvailable = false;
            updateAccepted = true;

            AppCastItem? downloadedItem = null;
            sparkle.CloseApplication += () => {
                Log.Information($"shutting down for update");
                CloseApplication?.Invoke();
                Log.Information($"shut down for update");
            };
            sparkle.DownloadStarted += (item, path) => {
                Log.Information($"download started {path}");
                downloadedItem = item;
            };
            sparkle.DownloadFinished += (item, path) => {
                Log.Information($"download finished {path}");
                // `item` is somehow null in this callback, likely a NetSparkle bug.
                item = item ?? downloadedItem;
                if (item == null)
                {
                    Log.Error("DownloadFinished unexpected null item.");
                }
                else
                {
                    sparkle.InstallUpdate(downloadedItem, path);
                }
            };
            sparkle.DownloadHadError += (item, path, e) => {
                Log.Error(e, $"download error {path}");
            };
            sparkle.DownloadMadeProgress += (sender, item, e) => {
                UpdaterStatus = $"{e.ProgressPercentage}%";
            };

            await sparkle.InitAndBeginDownload(updateInfo.Updates.First());
        }

        public void OnClosing()
        {
            if (!updateAccepted && updateInfo != null &&
                (updateInfo.Status == UpdateStatus.UpdateAvailable ||
                updateInfo.Status == UpdateStatus.UserSkipped) &&
                updateInfo.Updates.Count > 0)
            {
                Log.Information($"Skipping update {updateInfo.Updates[0].Version}");
                MainManager.Instance.Setting.SkipUpdate = updateInfo.Updates[0].Version.ToString();
                MainManager.Instance.Setting.Save();
            }
        }
    }

    // Force allow downgrading so that switching between beta and stable works.
    public class DowngradableFilter : IAppCastFilter
    {
        static bool Eq(int a, int b)
        {
            a = a == -1 ? 0 : a;
            b = b == -1 ? 0 : b;
            return a == b;
        }
        // Ambiguous version equal where 1.2 == 1.2.0 == 1.2.0.0.
        static bool Eq(Version a, Version b)
        {
            return Eq(a.Major, b.Major)
                && Eq(a.Minor, b.Minor)
                && Eq(a.Build, b.Build)
                && Eq(a.Revision, b.Revision);
        }
        public FilterResult GetFilteredAppCastItems(Version installed, List<AppCastItem> items)
        {
            items = items.Where(item => !Eq(new Version(item.Version), installed)).ToList();
            return new FilterResult(/*forceInstallOfLatestInFilteredList=*/true, items);
        }
    }

    public class ZipUpdater : SparkleUpdater
    {
        public ZipUpdater(string appcastUrl, ISignatureVerifier signatureVerifier) :
            base(appcastUrl, signatureVerifier)
        { }
        public ZipUpdater(string appcastUrl, ISignatureVerifier signatureVerifier, string referenceAssembly) :
            base(appcastUrl, signatureVerifier, referenceAssembly)
        { }
        public ZipUpdater(string appcastUrl, ISignatureVerifier signatureVerifier, string referenceAssembly, IUIFactory factory) :
            base(appcastUrl, signatureVerifier, referenceAssembly, factory)
        { }

        protected override string GetWindowsInstallerCommand(string downloadFilePath)
        {
            string installerExt = Path.GetExtension(downloadFilePath);
            if (DoExtensionsMatch(installerExt, ".exe"))
            {
                return $"\"{downloadFilePath}\"";
            }
            if (DoExtensionsMatch(installerExt, ".msi"))
            {
                return $"msiexec /i \"{downloadFilePath}\"";
            }
            if (DoExtensionsMatch(installerExt, ".msp"))
            {
                return $"msiexec /p \"{downloadFilePath}\"";
            }
            if (DoExtensionsMatch(installerExt, ".zip"))
            {
                string restart = RestartExecutablePath.TrimEnd('\\', '/');
                if (Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 17063)
                {
                    Log.Information("Starting update with tar.");
                    return $"tar -x -f \"{downloadFilePath}\" -C \"{restart}\"";
                }
                var unzipperPath = Path.Combine(Path.GetDirectoryName(downloadFilePath) ?? Path.GetTempPath(), "Unzipper.exe");
                File.WriteAllBytes(unzipperPath, Resources.Resources.Unzipper);
                Log.Information("Starting update with unzipper.");
                return $"{unzipperPath} \"{downloadFilePath}\" \"{restart}\"";
            }
            return downloadFilePath;
        }
    }
}