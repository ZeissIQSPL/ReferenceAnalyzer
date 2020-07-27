using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReferenceAnalyzer.UI.Models;
using ReferenceAnalyzer.UI.Services;
using ReferenceAnalyzer.UI.ViewModels;
using ReferenceAnalyzer.UI.Views;

namespace ReferenceAnalyzer.UI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var messageSink = new MessageSink();

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(
                        new Settings(),
                        new Core.ReferenceAnalyzer(messageSink),
                        messageSink),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
