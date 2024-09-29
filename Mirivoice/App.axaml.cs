using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Mirivoice.Mirivoice.Core;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        if (prefLang == null)
        {
            prefLang = sysLang;
            MainManager.Instance.Setting.Langcode = prefLang;
            MainManager.Instance.Setting.Save();
        }

        SetLanguage(prefLang);
       

        // Force using InvariantCulture to prevent issues caused by culture dependent string conversion, especially for floating point numbers.
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        Log.Information("Initialized culture.");
    }


    public static void SetLanguage(string language)
    {
        if (Current == null)
        {
            return;
        }
        var translations = App.Current.Resources.MergedDictionaries.OfType<ResourceInclude>().FirstOrDefault(x => x.Source?.OriginalString?.Contains("/Lang/") ?? false);

        if (!File.Exists( new Uri($"avares://Mirivoice.Main/Assets/Lang/{language}.axaml").LocalPath))
        {
            language = "en-US";
            MainManager.Instance.Setting.Langcode = language;
            MainManager.Instance.Setting.Save();
        }

        if (translations != null)
            App.Current.Resources.MergedDictionaries.Remove(translations);

        App.Current.Resources.MergedDictionaries.Add(
            new ResourceInclude(new Uri($"avares://Mirivoice.Main/Assets/Lang/{language}.axaml"))
            {
                Source = new Uri($"avares://Mirivoice.Main/Assets/Lang/{language}.axaml")
            });

    }

}
