using System.Linq;
using System.Reactive.Disposables;
using ReactiveUI;
using ReferenceExplorer.Tests;


namespace ReferenceExplorer.WPF
{
    public partial class MainWindow : ReactiveWindow<AppViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new AppViewModel(new RoslynProjectProvider(),new Settings());

            this.WhenActivated(disposableRegistration =>
            {
                this.Bind(ViewModel,
                        viewModel => viewModel.Path,
                        view => view.Path.Text)
                    .DisposeWith(disposableRegistration);

                this.BindCommand(ViewModel, viewModel => viewModel.Load,
                        view => view.Load)
                    .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Projects,
                        view => view.List.ItemsSource)
                    .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.SelectedProject.FormalReferences,
                        view => view.SelectedReferences.ItemsSource)
                    .DisposeWith(disposableRegistration);

                this.Bind(ViewModel, viewModel => viewModel.SelectedProject,
                        view => view.List.SelectedItem)
                    .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Count,
                        view => view.Count.Content,
                        count => $"number of projects: {count}")
                    .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Progress,
                        view => view.LoadingProgressBar.IsIndeterminate,
                        x => x < 0)
                    .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Progress,
                    view => view.LoadingProgressBar.Value)
                .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
		                viewModel => viewModel.SelectedProject.ActualReferences,
		                view => view.ActualReferences.ItemsSource)
	                .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
		                viewModel => viewModel.SelectedProject.DiffReferences,
		                view => view.DiffReferences.ItemsSource)
	                .DisposeWith(disposableRegistration);
            });
        }
    }
}