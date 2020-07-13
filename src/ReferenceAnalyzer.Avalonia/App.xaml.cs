using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReferenceAnalyzer.Avalonia.Models;
using ReferenceAnalyzer.Avalonia.ViewModels;
using ReferenceAnalyzer.Avalonia.Views;
using ReferenceAnalyzer.Core;

namespace ReferenceAnalyzer.Avalonia
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
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(
                        new Settings(),
                        new Core.ReferenceAnalyzer(
                            new MessageSink())),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
