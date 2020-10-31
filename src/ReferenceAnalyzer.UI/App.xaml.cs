using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReferenceAnalyzer.Core.ProjectEdit;
using ReferenceAnalyzer.Core.Util;
using ReferenceAnalyzer.UI.Models;
using ReferenceAnalyzer.UI.Resources;
using ReferenceAnalyzer.UI.Services;
using ReferenceAnalyzer.UI.ViewModels;
using ReferenceAnalyzer.UI.Views;
using Splat;

namespace ReferenceAnalyzer.UI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            Styles.Add(new Theme());
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Locator.CurrentMutable.Register(() => new SolutionView(), typeof(IViewFor<ISolutionViewModel>));
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var messageSink = new MessageSink();

                var referencesEditor = new ReferencesEditor(new ProjectAccess());
                var xamlReferencesReader = new XamlReferencesReader(new ProjectAccess());
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(
                        new SolutionViewModel(new Settings(), new SolutionFilepathPicker()),
                        new Core.ReferenceAnalyzer(
                            messageSink, referencesEditor, xamlReferencesReader),
                        referencesEditor,
                        messageSink)
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
