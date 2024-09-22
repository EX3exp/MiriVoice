using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Mirivoice.ViewModels;
using Mirivoice.Views;
using Mirivoice.Mirivoice.Core;
using Serilog;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Styling;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
namespace Mirivoice;


public partial class App : Application
{
    public override void Initialize()
    {
        Log.Information("App Initialize");
        AvaloniaXamlLoader.Load(this);
        InitializeCulture();
    }

    public override void OnFrameworkInitializationCompleted()
    {


        //Lang.Resources.Culture = new CultureInfo("ko-KR");
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var args = Environment.GetCommandLineArgs();
            var mainWindow = new MainWindow();
            MainViewModel mainviewModel = new MainViewModel(mainWindow);


            if (args.Length > 1)
            {
                Log.Information($"Open project from command line argument -- {args[1]}");
                string filePath = args[1]; // read file path from command line argument

                if (File.Exists(filePath))
                {

                    var result = Task.Run(async () => await mainviewModel.OpenProject(filePath));
                    
                }
            }
            mainviewModel.OnPropertyChanged((mainviewModel.Title));
            mainWindow.DataContext = mainviewModel;
            desktop.MainWindow = mainWindow; 
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    public void InitializeCulture()
    {
        Log.Information("Initializing culture.");
        string sysLang = CultureInfo.InstalledUICulture.Name;
        string prefLang = MainManager.Instance.Setting.Langcode;
        var languages = GetLanguages();
        if (languages.ContainsKey(prefLang))
        {
            SetLanguage(prefLang);
        }
        else if (languages.ContainsKey(sysLang))
        {
            SetLanguage(sysLang);
            MainManager.Instance.Setting.Langcode = sysLang;
            MainManager.Instance.Setting.Save();
        }
        else
        {
            SetLanguage("en-US");
        }

        // Force using InvariantCulture to prevent issues caused by culture dependent string conversion, especially for floating point numbers.
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        Log.Information("Initialized culture.");
    }

    public static Dictionary<string, IResourceProvider> GetLanguages()
    {
        if (Current == null)
        {
            return new();
        }
        var result = new Dictionary<string, IResourceProvider>();
        foreach (string key in Current.Resources.Keys.OfType<string>())
        {
            if (key.StartsWith("strings-") &&
                Current.Resources.TryGetResource(key, ThemeVariant.Default, out var res) &&
                res is IResourceProvider rp)
            {
                result.Add(key.Replace("strings-", ""), rp);
            }
        }
        return result;
    }

    public static void SetLanguage(string language)
    {
        if (Current == null)
        {
            return;
        }
        var languages = GetLanguages();
        foreach (var res in languages.Values)
        {
            Current.Resources.MergedDictionaries.Remove(res);
        }
        if (language != "en-US")
        {
            Current.Resources.MergedDictionaries.Add(languages["en-US"]);
        }
        if (languages.TryGetValue(language, out var res1))
        {
            Current.Resources.MergedDictionaries.Add(res1);
        }
    }

}
