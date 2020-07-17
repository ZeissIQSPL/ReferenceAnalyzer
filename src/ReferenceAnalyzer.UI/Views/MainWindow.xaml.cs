using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using MessageBox.Avalonia;
using ReactiveUI;
using ReferenceAnalyzer.UI.ViewModels;

namespace ReferenceAnalyzer.UI.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);


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
                        var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow("warning", interaction.Input);
                        messageBoxStandardWindow.Show();
                        interaction.SetOutput(Unit.Default);
                    });

                BindCommands(disposableRegistration);
                BindLists(disposableRegistration);
            });
        }

        public TextBox Path => this.FindControl<TextBox>(nameof(Path));
        public CheckBox StopOnError => this.FindControl<CheckBox>(nameof(StopOnError));
        public ListBox Projects => this.FindControl<ListBox>(nameof(Projects));
        public ListBox ActualReferences => this.FindControl<ListBox>(nameof(ActualReferences));
        public ListBox DefinedReferences => this.FindControl<ListBox>(nameof(DefinedReferences));
        public ListBox DiffReferences => this.FindControl<ListBox>(nameof(DiffReferences));
        public Button LoadCommand => this.FindControl<Button>(nameof(LoadCommand));
        public Button AnalyzeCommand => this.FindControl<Button>(nameof(AnalyzeCommand));


        private void BindLists(CompositeDisposable disposableRegistration)
        {
            this.Bind(ViewModel, viewModel => viewModel.SelectedProject,
                    view => view.Projects.SelectedItem)
                .DisposeWith(disposableRegistration);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.Projects,
                    view => view.Projects.Items)
                .DisposeWith(disposableRegistration);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.SelectedProjectReport.ActualReferences,
                    view => view.ActualReferences.Items)
                .DisposeWith(disposableRegistration);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.SelectedProjectReport.DefinedReferences,
                    view => view.DefinedReferences.Items)
                .DisposeWith(disposableRegistration);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.SelectedProjectReport.DiffReferences,
                    view => view.DiffReferences.Items)
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
