﻿using Avalonia;
using Avalonia.ReactiveUI;
using Mirivoice.Mirivoice.Core;
using Serilog;
using System;
using System.IO;
using System.Text;

namespace Mirivoice.Desktop;

class Program
{

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) 
    {
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitLogging();
            InitMirivoice();
            
            
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Log.Error($"{e.Message}\n{e.StackTrace}");
        }
        finally
        {
            Log.CloseAndFlush();
        }

    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI()
            .UseR3();

    public static void InitLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Debug()
            .WriteTo.Logger(lc => lc
                .MinimumLevel.Information()
                .WriteTo.File(MainManager.Instance.PathM.LogFilePath, rollingInterval: RollingInterval.Day, encoding: Encoding.UTF8))
            .CreateLogger();

        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, args) => {
            Log.Error((Exception)args.ExceptionObject, "Unhandled exception");
        });
        Log.Information("Logging initialized.");
    }

    public static void InitMirivoice()
    {
        
        Log.Information("Mirivoice init");
        MainManager.Instance.Initialize();


    }

    
}
