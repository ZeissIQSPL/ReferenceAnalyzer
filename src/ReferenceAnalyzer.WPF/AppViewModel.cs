using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReferenceAnalyzer.Core;
using ReferenceAnalyzer.WPF.Utilities;

namespace ReferenceAnalyzer.WPF
{
    public class AppViewModel : ReactiveObject
    {
        private readonly ReadOnlyObservableCollection<ReferencesReport> _reports;
        private readonly ReadOnlyObservableCollection<string> _projects;
        private string _path;
        private bool _stopOnError = true;
        private string _selectedProject;

        public AppViewModel(ISettings settings, IReferenceAnalyzer projectProvider)
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

            var canLoad = this.WhenAnyValue(x => x.Path,
                    path => !string.IsNullOrEmpty(path));

            Load = ReactiveCommand.CreateFromObservable(() =>
                Observable.Create<string>(o =>
                    LoadProjects(projectProvider, o)),
                canLoad);

            Load.ToObservableChangeSet()
                .Bind(out _projects)
                .Subscribe();

            Load.ThrownExceptions.Subscribe(error => MessageBox.Show("Error caught: " + error.Message));

            var canAnalyze = Projects.ToObservableChangeSet()
                .Select(_ => Projects?.Any() == true);

            Analyze = ReactiveCommand.CreateFromObservable<IEnumerable<string>, ReferencesReport>(projects =>
                    Observable.Create<ReferencesReport>(o =>
                    LoadReferencesReports(projectProvider, o, projects)),
                canAnalyze);

            Analyze.ToObservableChangeSet()
                .Bind(out _reports)
                .Subscribe();

            this.WhenAnyValue(viewModel => viewModel.Path)
                .Subscribe(x => settings.SolutionPath = x);

            this.WhenAnyValue(viewModel => viewModel.StopOnError)
                .Subscribe(x => projectProvider.ThrowOnCompilationFailures = x);

            this.WhenAnyValue(viewModel => viewModel.SelectedProject, viewModel => viewModel.Reports.Count)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(SelectedProjectReport)));

        }

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

        public ReactiveCommand<Unit, string> Load { get; }
        public bool StopOnError
        {
            get => _stopOnError;
            set => this.RaiseAndSetIfChanged(ref _stopOnError, value);
        }

        public ReactiveCommand<IEnumerable<string>, ReferencesReport> Analyze { get; }

        private async Task LoadProjects(IReferenceAnalyzer projectProvider, IObserver<string> o)
        {
            foreach (var element in await projectProvider.Load(Path))
                o.OnNext(element);
        }

        private static async Task LoadReferencesReports(IReferenceAnalyzer projectProvider, IObserver<ReferencesReport> o, IEnumerable<string> projects)
        {
            await foreach (var element in projectProvider.Analyze(projects))
                o.OnNext(element);
        }
    }
}
