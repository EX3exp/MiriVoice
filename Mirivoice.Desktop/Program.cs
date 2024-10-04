using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Mirivoice.Mirivoice.Core;
using Mirivoice.Mirivoice.Core.Managers;
using Mirivoice.Mirivoice.Core.Utils;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        InitLogging();
        string processName = Process.GetCurrentProcess().ProcessName;
        if (processName != "dotnet")
        {
            var exists = Process.GetProcessesByName(processName).Count() > 1;
            if (exists)
            {
                Log.Information($"Process {processName} already open. Exiting.");
                return;
            }
        }
        Log.Information($"{Environment.OSVersion}");
        Log.Information($"{RuntimeInformation.OSDescription} " +
            $"{RuntimeInformation.OSArchitecture} " +
            $"{RuntimeInformation.ProcessArchitecture}");
        Log.Information($"MiriVoice v{Assembly.GetEntryAssembly()?.GetName().Version} " +
            $"{RuntimeInformation.RuntimeIdentifier}");
        Log.Information($"Data path = {MainManager.Instance.PathM.DataPath}");
        Log.Information($"Cache path = {MainManager.Instance.PathM.CachePath}");
        try
        {
            Run(args);
            Log.Information($"Exiting.");
        }
        catch (Exception e)
        {
            Log.Error(e, "Unhandled exception");
        }
        Log.Information($"Exited.");

    }

    public static void Run(string[] args)
            => BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(
                    args, ShutdownMode.OnMainWindowClose);


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI()
            .UseR3()
            .With(new X11PlatformOptions {EnableIme = true});

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

    

    
}
