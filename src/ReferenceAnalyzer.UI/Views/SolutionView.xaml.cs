using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using MessageBox.Avalonia;
using ReactiveUI;
using ReferenceAnalyzer.UI.ViewModels;

namespace ReferenceAnalyzer.UI.Views
{

    public class SolutionView : ReactiveUserControl<SolutionViewModel>
    {
        public SolutionView()
        {
            this.WhenActivated(disposableRegistration =>
            {
                this.Bind(ViewModel,
                       viewModel => viewModel.Path,
                       view => view.Path.Text)
                   .DisposeWith(disposableRegistration);
            });

            AvaloniaXamlLoader.Load(this);
        }

        public TextBox Path => this.FindControl<TextBox>(nameof(Path));

    }
}
