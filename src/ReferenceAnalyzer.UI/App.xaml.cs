using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReferenceAnalyzer.Core;
using ReferenceAnalyzer.Core.ProjectEdit;
using ReferenceAnalyzer.UI.Models;
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
                var referencesEditor = new ReferencesEditor(new ProjectAccess());
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(
                        new Settings(),
                        new Core.ReferenceAnalyzer(
                            new MessageSink(), referencesEditor),
                        referencesEditor),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
