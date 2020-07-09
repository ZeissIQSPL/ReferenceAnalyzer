using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using DynamicData;
using ReactiveUI;
using ReferenceAnalyzer.Core;
using ReferenceAnalyzer.WPF.Utilities;

namespace ReferenceAnalyzer.WPF
{
    public class AppViewModel : ReactiveObject
    {
        private readonly ReadOnlyObservableCollection<ReferencesReport> _projects;
        private string _path;
        private ReferencesReport _selectedProject;
        private bool _stopOnError = true;

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

            var canLoad =
                this.WhenAnyValue(x => x.Path,
                    path => !string.IsNullOrEmpty(path));

            Load = ReactiveCommand.CreateFromObservable(() =>
                Observable.Create<ReferencesReport>(o =>
                    LoadReferencesReports(projectProvider, o)),
                canLoad);

            Load.ThrownExceptions.Subscribe(error => MessageBox.Show("Error caught: " + error.Message));

            Load.ToObservableChangeSet()
                .Bind(out _projects)
                .Subscribe();

            this.WhenAnyValue(viewModel => viewModel.Path)
                .Subscribe(x => settings.SolutionPath = x);

            this.WhenAnyValue(viewModel => viewModel.StopOnError)
                .Subscribe(x => projectProvider.ThrowOnCompilationFailures = x);
        }

        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        public ReferencesReport SelectedProject
        {
            get => _selectedProject;
            set => this.RaiseAndSetIfChanged(ref _selectedProject, value);
        }


        public ReadOnlyObservableCollection<ReferencesReport> Projects => _projects;

        public ReactiveCommand<Unit, ReferencesReport> Load { get; }
        public bool StopOnError
        {
            get => _stopOnError;
            set => this.RaiseAndSetIfChanged(ref _stopOnError, value);
        }

        private async Task LoadReferencesReports(IReferenceAnalyzer projectProvider, IObserver<ReferencesReport> o)
        {
            await foreach (var element in projectProvider.AnalyzeAll(Path))
                o.OnNext(element);
        }
    }
}
