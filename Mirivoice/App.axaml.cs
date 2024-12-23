﻿using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Mirivoice.Mirivoice.Core;
using Mirivoice.ViewModels;
using Mirivoice.Views;
using Serilog;
using System;
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
        InitMirivoice();
        InitializeComponent();
        InitializeCulture();
    }
    public static void InitMirivoice()
    {

        Log.Information("Mirivoice init");
        MainManager.Instance.Initialize();


    }
    public static string[] Languages = new string[] { "en-US", "ko-KR"};

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    

    public override void OnFrameworkInitializationCompleted()
    {


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
            mainWindow.Content = new MainView(mainviewModel);
            mainWindow.DataContext = mainviewModel;
            mainviewModel.OnPropertyChanged((mainviewModel.Title));
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

        if (Languages.Contains(language) == false)
        {
            Log.Warning($"Language {language} is not supported. Defaulting to en-US.");
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
