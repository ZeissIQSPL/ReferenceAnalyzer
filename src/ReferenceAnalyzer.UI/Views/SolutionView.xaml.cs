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

    public class SolutionView : ReactiveUserControl<ISolutionViewModel>
    {
        public SolutionView()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposableRegistration =>
            {
                this.Bind(ViewModel,
                       viewModel => viewModel.Path,
                       view => view.Path.Text)
                   .DisposeWith(disposableRegistration);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.PickSolutionFile,
                    view => view.PickSolutionLocation)
                .DisposeWith(disposableRegistration);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.ClearSolutionList,
                    view => view.ClearList)
                .DisposeWith(disposableRegistration);

                this.Bind(ViewModel,
                    viewModel => viewModel.SelectedPath,
                    view => view.LastLoadedSolutions.SelectedItem)
                .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.LastSolutions,
                    view => view.LastLoadedSolutions.Items)
                .DisposeWith(disposableRegistration);


            });

        }

        public TextBlock Path => this.FindControl<TextBlock>(nameof(Path));
        public Button PickSolutionLocation => this.FindControl<Button>(nameof(PickSolutionLocation));
        public Button ClearList => this.FindControl<Button>(nameof(ClearList));
        public ListBox LastLoadedSolutions => this.FindControl<ListBox>(nameof(LastLoadedSolutions));
    }
}
