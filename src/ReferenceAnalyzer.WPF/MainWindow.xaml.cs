using System.Reactive;
using System.Reactive.Disposables;
using System.Windows;
using ReactiveUI;
using ReferenceAnalyzer.Core;
using ReferenceAnalyzer.WPF.Utilities;

namespace ReferenceAnalyzer.WPF
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ReactiveWindow<AppViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new AppViewModel(
                new Settings(),
                new Core.ReferenceAnalyzer(
                    new MessageSink()));

            this.WhenActivated(disposableRegistration =>
            {
                this.Bind(ViewModel,
                    viewModel => viewModel.Path,
                    view => view.Path.Text)
                    .DisposeWith(disposableRegistration);

                this.Bind(ViewModel,
                        viewModel => viewModel.StopOnError,
                        view => view.StopOnError.IsChecked)
                    .DisposeWith(disposableRegistration);

                ViewModel.MessagePopup
                    .RegisterHandler(interaction =>
                    {
                        MessageBox.Show(interaction.Input);
                        interaction.SetOutput(Unit.Default);
                    });

                BindCommands(disposableRegistration);
                BindLists(disposableRegistration);
            });
        }

        private void BindLists(CompositeDisposable disposableRegistration)
        {
            this.Bind(ViewModel, viewModel => viewModel.SelectedProject,
                    view => view.Projects.SelectedItem)
                .DisposeWith(disposableRegistration);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.Projects,
                    view => view.Projects.ItemsSource)
                .DisposeWith(disposableRegistration);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.SelectedProjectReport.ActualReferences,
                    view => view.ActualReferences.ItemsSource)
                .DisposeWith(disposableRegistration);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.SelectedProjectReport.DefinedReferences,
                    view => view.DefinedReferences.ItemsSource)
                .DisposeWith(disposableRegistration);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.SelectedProjectReport.DiffReferences,
                    view => view.DiffReferences.ItemsSource)
                .DisposeWith(disposableRegistration);
        }

        private void BindCommands(CompositeDisposable disposableRegistration)
        {
            this.BindCommand(ViewModel,
                    viewModel => viewModel.Load,
                    view => view.LoadCommand)
                .DisposeWith(disposableRegistration);

            this.BindCommand(ViewModel,
                    viewModel => viewModel.Analyze,
                    view => view.AnalyzeCommand,
                    viewModel => viewModel.Projects)
                .DisposeWith(disposableRegistration);
        }
    }
}
