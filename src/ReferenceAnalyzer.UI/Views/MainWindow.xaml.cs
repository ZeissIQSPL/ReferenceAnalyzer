using System.Linq;
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

                BindProgress(disposableRegistration);

                ViewModel.MessagePopup
                    .RegisterHandler(interaction =>
                    {
                        var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow("warning", interaction.Input);
                        messageBoxStandardWindow.Show();
                        interaction.SetOutput(Unit.Default);
                    });

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Log,
                        view => view.Logs.Text,
                        lines => lines)
                    .DisposeWith(disposableRegistration);

                this.Bind(ViewModel,
                        viewModel => viewModel.Whitelist,
                        view => view.Whitelist.Text)
                    .DisposeWith(disposableRegistration);

                BindCommands(disposableRegistration);
                BindLists(disposableRegistration);
            });
        }

        private void BindProgress(CompositeDisposable disposableRegistration)
        {
            this.OneWayBind(ViewModel,
                    viewModel => viewModel.Progress,
                    view => view.Progress.Value)
                .DisposeWith(disposableRegistration);

            this.OneWayBind(ViewModel,
                    viewModel => viewModel.Progress,
                    view => view.Progress.IsIndeterminate,
                    p => p == -1)
                .DisposeWith(disposableRegistration);
        }

        public TextBox Path => this.FindControl<TextBox>(nameof(Path));
        public CheckBox StopOnError => this.FindControl<CheckBox>(nameof(StopOnError));
        public ListBox Projects => this.FindControl<ListBox>(nameof(Projects));
        public ListBox ActualReferences => this.FindControl<ListBox>(nameof(ActualReferences));
        public ListBox DefinedReferences => this.FindControl<ListBox>(nameof(DefinedReferences));
        public ListBox DiffReferences => this.FindControl<ListBox>(nameof(DiffReferences));
        public Button LoadCommand => this.FindControl<Button>(nameof(LoadCommand));
        public Button AnalyzeAllCommand => this.FindControl<Button>(nameof(AnalyzeAllCommand));
        public Button AnalyzeSelectedCommand => this.FindControl<Button>(nameof(AnalyzeSelectedCommand));
        public ProgressBar Progress => this.FindControl<ProgressBar>(nameof(Progress));
        public CheckBox IncludeNuGets => this.FindControl<CheckBox>(nameof(IncludeNuGets));
        public Button RemoveUnusedCommand => this.FindControl<Button>(nameof(RemoveUnusedCommand));
        public Button RemoveAllUnusedCommand => this.FindControl<Button>(nameof(RemoveAllUnusedCommand));
        public TextBlock Logs => this.FindControl<TextBlock>(nameof(Logs));
        public TextBox Whitelist => this.FindControl<TextBox>(nameof(Whitelist));


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
                    view => view.AnalyzeAllCommand,
                    viewModel => viewModel.Projects)
                .DisposeWith(disposableRegistration);

            this.BindCommand(ViewModel,
                    viewModel => viewModel.AnalyzeSelected,
                    view => view.AnalyzeSelectedCommand,
                    viewModel => viewModel.SelectedProject)
                .DisposeWith(disposableRegistration);

            this.BindCommand(ViewModel,
                    viewModel => viewModel.RemoveUnused,
                    view => view.RemoveUnusedCommand,
                    viewModel => viewModel.SelectedProjectReport)
                .DisposeWith(disposableRegistration);

            this.BindCommand(ViewModel,
                    viewModel => viewModel.RemoveAllUnused,
                    view => view.RemoveAllUnusedCommand,
                    viewModel => viewModel.Reports)
                .DisposeWith(disposableRegistration);
        }
    }
}
