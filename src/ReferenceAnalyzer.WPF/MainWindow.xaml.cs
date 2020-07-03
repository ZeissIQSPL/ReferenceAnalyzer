using ReactiveUI;
using ReferenceAnalyzer.WPF.Utilities;
using System.Linq;
using System.Reactive.Disposables;
using ReferenceAnalyzer.Core;

namespace ReferenceAnalyzer.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ReactiveWindow<AppViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new AppViewModel(new Settings(), new ReferenceAnalyzer.Core.ReferenceAnalyzer(new MessageSink()));

            this.WhenActivated(disposableRegistration =>
            {
                this.Bind(ViewModel,
                    viewModel => viewModel.Path,
                    view => view.Path.Text
                    ).DisposeWith(disposableRegistration);

                this.BindCommand(ViewModel, viewModel => viewModel.Load, view => view.LoadCommand).DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel, viewModel => viewModel.Projects, view => view.Projects.ItemsSource).DisposeWith(disposableRegistration);

                this.Bind(ViewModel, viewModel => viewModel.SelectedProject,
                        view => view.Projects.SelectedItem)
                    .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel, viewModel => viewModel.SelectedProject.ActualReferences, view => view.ActualReferences.ItemsSource).DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel, viewModel => viewModel.SelectedProject.DefinedReferences, view => view.DefinedReferences.ItemsSource).DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel, viewModel => viewModel.SelectedProject.DiffReferences, view => view.DiffReferences.ItemsSource).DisposeWith(disposableRegistration);
            });
        }
    }
}
