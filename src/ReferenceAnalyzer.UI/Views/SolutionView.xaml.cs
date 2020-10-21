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


                this.OneWayBind(ViewModel,
                        viewModel => viewModel.LastSolutions,
                        view => view.LastLoadedSolutions.Items)
                    .DisposeWith(disposableRegistration);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.PickSolutionFile,
                    view => view.PickSolutionLocation)
                .DisposeWith(disposableRegistration);
            });

            AvaloniaXamlLoader.Load(this);
        }

        public TextBox Path => this.FindControl<TextBox>(nameof(Path));
        public Button PickSolutionLocation => this.FindControl<Button>(nameof(PickSolutionLocation));
        public ListBox LastLoadedSolutions => this.FindControl<ListBox>(nameof(LastLoadedSolutions));
    }
}
