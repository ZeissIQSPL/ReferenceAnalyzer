using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReferenceAnalyzer.Core;
using ReferenceAnalyzer.Avalonia.Models;

namespace ReferenceAnalyzer.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ReadOnlyObservableCollection<ReferencesReport> _reports;
        private ReadOnlyObservableCollection<string> _projects;
        private string _path;
        private bool _stopOnError = true;
        private string _selectedProject;

        public MainWindowViewModel(ISettings settings, IReferenceAnalyzer projectProvider)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (projectProvider == null)
                throw new ArgumentNullException(nameof(projectProvider));

            projectProvider.BuildProperties = new Dictionary<string, string>
            {
                {"AlwaysCompileMarkupFilesInSeparateDomain", "false"},
                {"Configuration", "Debug"},
                {"Platform", "x64"}
            };

            _path = settings.SolutionPath;

            SetupCommands(projectProvider);

            SetupProperties(settings, projectProvider);
        }

        private void SetupProperties(ISettings settings, IReferenceAnalyzer projectProvider)
        {
            this.WhenAnyValue(viewModel => viewModel.Path)
                .Subscribe(x => settings.SolutionPath = x);

            this.WhenAnyValue(viewModel => viewModel.StopOnError)
                .Subscribe(x => projectProvider.ThrowOnCompilationFailures = x);

            this.WhenAnyValue(viewModel => viewModel.SelectedProject, viewModel => viewModel.Reports.Count)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(SelectedProjectReport)));
        }

        private void SetupCommands(IReferenceAnalyzer projectProvider)
        {
            var canLoad = this.WhenAnyValue(x => x.Path,
                path => !string.IsNullOrEmpty(path));

            Load = ReactiveCommand.CreateFromTask(() =>
                LoadProjects(projectProvider),
                canLoad);

            Load.ThrownExceptions
                .Subscribe(async error => await MessagePopup.Handle(error.Message));

            Load.ToObservableChangeSet()
                .Bind(out _projects)
                .Subscribe();

            var canAnalyze = Projects.ToObservableChangeSet()
                .Select(_ => Projects?.Any() == true);

            Analyze = ReactiveCommand.CreateFromObservable<IEnumerable<string>, ReferencesReport>(projects =>
                    Observable.Create<ReferencesReport>(o =>
                        LoadReferencesReports(projectProvider, o, projects)),
                canAnalyze);

            Analyze.ToObservableChangeSet()
                .Bind(out _reports)
                .Subscribe();
        }

        public Interaction<string, Unit> MessagePopup { get; } = new Interaction<string, Unit>();

        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        public ReferencesReport SelectedProjectReport => Reports.FirstOrDefault(r => r.Project == SelectedProject);

        public string SelectedProject
        {
            get => _selectedProject;
            set => this.RaiseAndSetIfChanged(ref _selectedProject, value);
        }

        public ReadOnlyObservableCollection<ReferencesReport> Reports => _reports;
        public ReadOnlyObservableCollection<string> Projects => _projects;

        public ReactiveCommand<Unit, IEnumerable<string>> Load { get; set; }

        public bool StopOnError
        {
            get => _stopOnError;
            set => this.RaiseAndSetIfChanged(ref _stopOnError, value);
        }

        public ReactiveCommand<IEnumerable<string>, ReferencesReport> Analyze { get; set; }

        private async Task<IEnumerable<string>> LoadProjects(IReferenceAnalyzer projectProvider) => await projectProvider.Load(Path);

        private static async Task LoadReferencesReports(IReferenceAnalyzer projectProvider,
            IObserver<ReferencesReport> observer,
            IEnumerable<string> projects)
        {
            await foreach (var element in projectProvider.Analyze(projects))
                observer.OnNext(element);
            observer.OnCompleted();
        }
    }
}
