using MainCore;
using MainCore.Services;
using MainCore.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveMarbles.Extensions.Hosting.ReactiveUI;
using ReactiveMarbles.Extensions.Hosting.Wpf;
using ReactiveUI;
using Splat.ModeDetection;
using Serilog;
using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WPFUI.Views;

namespace WPFUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            Splat.ModeDetector.OverrideModeDetector(Mode.Run);
            var host = AppMixins.GetHostBuilder()
                .ConfigureWpf(wpfBuilder => wpfBuilder.UseCurrentApplication(this).UseWindow<MainWindow>())
                .UseWpfLifetime()
                .Build();

            host.MapSplatLocator(sp =>
            {
                RxApp.DefaultExceptionHandler = sp.GetRequiredService<ObservableExceptionHandler>();
                SetupDialogService(sp);
                sp.GetRequiredService<IRxQueue>().Setup();
            });

            host.RunAsync();
        }

        private static void LogFatalException(Exception ex, string source)
        {
            var message = $"[{source}] {ex}";
            var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "crash.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
            Log.Fatal(ex, "Unhandled exception from {Source}", source);
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogFatalException(e.Exception, "Dispatcher");
            MessageBox.Show($"An unexpected error occurred:\n{e.Exception.Message}\n\nPlease check logs/crash.txt for details.", "Centurion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogFatalException(ex, "AppDomain");
            }
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogFatalException(e.Exception, "TaskScheduler");
            e.SetObserved();
        }

        private static void SetupDialogService(IServiceProvider serviceProvider)
        {
            var dialogService = serviceProvider.GetRequiredService<IDialogService>();
            dialogService.MessageBox.RegisterHandler(async context =>
            {
                ShowMessage(context.Input.Title, context.Input.Message);
                context.SetOutput(Unit.Default);
                await Task.CompletedTask;
            });

            dialogService.ConfirmBox.RegisterHandler(async context =>
            {
                var result = ShowConfirm(context.Input.Title, context.Input.Message);
                context.SetOutput(result);
                await Task.CompletedTask;
            });

            dialogService.OpenFileDialog.RegisterHandler(async context =>
            {
                var result = OpenFileDialog();
                context.SetOutput(result);
                await Task.CompletedTask;
            });

            dialogService.SaveFileDialog.RegisterHandler(async context =>
            {
                var result = SaveFileDialog();
                context.SetOutput(result);
                await Task.CompletedTask;
            });
        }

        private static void ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title);
        }

        private static bool ShowConfirm(string title, string message)
        {
            var answer = MessageBox.Show(message, title, MessageBoxButton.YesNo);
            return answer == MessageBoxResult.Yes;
        }

        private static string SaveFileDialog()
        {
            var svd = new Microsoft.Win32.SaveFileDialog
            {
                InitialDirectory = AppContext.BaseDirectory,
                Filter = "Centurion files (*.centurion)|*.centurion|All files (*.*)|*.*",
            };
            if (svd.ShowDialog() != true) return "";
            return svd.FileName;
        }

        private static string OpenFileDialog()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = AppContext.BaseDirectory,
                Filter = "Centurion files (*.centurion)|*.centurion|All files (*.*)|*.*",
            };
            if (ofd.ShowDialog() != true) return "";
            return ofd.FileName;
        }
    }
}