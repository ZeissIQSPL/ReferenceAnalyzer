using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReferenceAnalyzer.Core;
using ReferenceAnalyzer.Core.ProjectEdit;
using ReferenceAnalyzer.UI.Models;
using ReferenceAnalyzer.UI.Services;

namespace ReferenceAnalyzer.UI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private ReadOnlyObservableCollection<ReferencesReport> _reports;
        private ReadOnlyObservableCollection<string> _projects;
        private string _path;
        private bool _stopOnError;
        private string _selectedProject;
        private double _progress;
        private string _log;
        private string _whitelist;
        private CancellationTokenSource _tokenSource;

        public MainWindowViewModel(ISettings settings, IReferenceAnalyzer analyzer, IReferencesEditor editor, IReadableMessageSink messageSink)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (analyzer == null)
                throw new ArgumentNullException(nameof(analyzer));
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));
            if (messageSink == null)
                throw new ArgumentNullException(nameof(messageSink));

            _tokenSource = new CancellationTokenSource();

            ConfigureAnalyzer(analyzer);
            SetupSettings(settings);
            SetupCommands(analyzer, editor);
            SetupProperties(analyzer);
            SetupSink(messageSink);
        }

        public IReadableMessageSink MessageSink { get; set; }

        public ReadOnlyObservableCollection<ReferencesReport> Reports => _reports;
        public ReadOnlyObservableCollection<string> Projects => _projects;

        public ReactiveCommand<Unit, IEnumerable<string>> Load { get; private set; }
        public ReactiveCommand<Unit, Unit> Cancel { get; private set; }
        public ReactiveCommand<IEnumerable<string>, ReferencesReport> Analyze { get; private set; }
        public ReactiveCommand<string, ReferencesReport> AnalyzeSelected { get; set; }
        public ReactiveCommand<ReferencesReport, Unit> RemoveUnused { get; private set; }
        public ReactiveCommand<IEnumerable<ReferencesReport>, Unit> RemoveAllUnused { get; set; }

        public Interaction<string, Unit> MessagePopup { get; } = new Interaction<string, Unit>();

        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        public ReferencesReport SelectedProjectReport =>
            Reports.FirstOrDefault(r => r.Project == SelectedProject) ?? ReferencesReport.Empty();

        public string SelectedProject
        {
            get => _selectedProject;
            set => this.RaiseAndSetIfChanged(ref _selectedProject, value);
        }

        public bool StopOnError
        {
            get => _stopOnError;
            set => this.RaiseAndSetIfChanged(ref _stopOnError, value);
        }

        public double Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        public string Log
        {
            get => _log;
            set => this.RaiseAndSetIfChanged(ref _log, value);
        }

        public string Whitelist
        {
            get => _whitelist;
            set => this.RaiseAndSetIfChanged(ref _whitelist, value);
        }

        private void ConfigureAnalyzer(IReferenceAnalyzer analyzer)
        {
            analyzer.BuildProperties = new Dictionary<string, string>
            {
                {"AlwaysCompileMarkupFilesInSeparateDomain", "false"},
                {"Configuration", "Debug"},
                {"Platform", "x64"},
                {"VCTargetsPath", @"C:\Program Files (x86)\MSBuild\Microsoft.Cpp\v4.0\"}
            };

            analyzer.ProgressReporter = new Progress<double>(p => Progress = p);
        }

        private void SetupSink(IReadableMessageSink messageSink)
        {
            MessageSink = messageSink;

            MessageSink.Lines.ToObservableChangeSet()
                .Select(_ => MessageSink.Lines)
                .Subscribe(lines => Log = string.Join('\n', lines));
        }

        private void SetupSettings(ISettings settings)
        {
            _path = settings.SolutionPath;

            this.WhenAnyValue(viewModel => viewModel.Path)
                .Subscribe(x => settings.SolutionPath = x);
        }

        private void SetupProperties(IReferenceAnalyzer analyzer)
        {
            this.WhenAnyValue(viewModel => viewModel.StopOnError)
                .Subscribe(x => analyzer.ThrowOnCompilationFailures = x);

            this.WhenAnyValue(viewModel => viewModel.SelectedProject, viewModel => viewModel.Reports.Count)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(SelectedProjectReport)));
        }

        private void SetupCommands(IReferenceAnalyzer projectProvider, IReferencesEditor editor)
        {
            SetupLoad(projectProvider);
            SetupAnalyze(projectProvider);
            SetupRemove(editor);

            Cancel = ReactiveCommand.Create(() =>
            {
                _tokenSource.Cancel();
                _tokenSource = new CancellationTokenSource();
            });
        }

        private void SetupLoad(IReferenceAnalyzer projectProvider)
        {
            var canLoad = this.WhenAnyValue(x => x.Path,
                path => !string.IsNullOrEmpty(path));

            Load = ReactiveCommand.CreateFromTask(() =>
                    LoadProjects(projectProvider),
                canLoad);

            Load.ThrownExceptions
                .Subscribe(HandleCommandExceptions);

            var projects = new SourceList<string>();

            Load.Subscribe(p => projects.AddRange(p));
            Load.IsExecuting
                .Where(executionStarted => executionStarted)
                .Subscribe(_ => projects.Clear());

            projects.Connect()
                .Bind(out _projects)
                .Subscribe();
        }

        private async void HandleCommandExceptions(Exception error)
        {
            if (error is OperationCanceledException)
                return;

            await MessagePopup.Handle(error.Message);
        }

        private void SetupAnalyze(IReferenceAnalyzer analyzer)
        {
            var canAnalyze = Projects.ToObservableChangeSet()
                .Select(_ => Projects?.Any() == true);

            Analyze = ReactiveCommand.CreateFromObservable<IEnumerable<string>, ReferencesReport>(projects =>
                    Observable.Create<ReferencesReport>(o =>
                        AnalyzeReferences(analyzer, o, projects)),
                canAnalyze);

            var reports = new SourceList<ReferencesReport>();

            Analyze.Subscribe(r => reports.Add(r));
            Analyze.IsExecuting
                .Where(e => e)
                .Subscribe(_ => reports.Clear());

            Analyze.ThrownExceptions
                .Subscribe(HandleCommandExceptions);

            reports.Connect()
                .Bind(out _reports)
                .Subscribe();

            var canAnalyzeSelected = this.WhenAnyValue(vm => vm.SelectedProject)
                .Select(p => !string.IsNullOrEmpty(p));

            AnalyzeSelected = ReactiveCommand.CreateFromObservable<string, ReferencesReport>(p =>
                Analyze.Execute(new[] {p}), canAnalyzeSelected);
        }

        private void SetupRemove(IReferencesEditor editor)
        {
            var canRemove = this.WhenAnyValue(x => x.SelectedProjectReport,
                report => report.DiffReferences.Any());

            RemoveUnused = ReactiveCommand.CreateFromTask<ReferencesReport, Unit>(report =>
                    RemoveReferences(report, editor),
                canRemove);

            RemoveAllUnused = ReactiveCommand.CreateFromTask<IEnumerable<ReferencesReport>, Unit>(async reports =>
                {
                    await Task.WhenAll(reports.Select(report => RemoveReferences(report, editor)));
                    return Unit.Default;
                },
                this.WhenAnyValue(x => x.Reports.Count, count => count > 0));
        }

        private Task<Unit> RemoveReferences(ReferencesReport report, IReferencesEditor editor)
        {
            editor.RemoveReferencedProjects(report.ProjectPath, report.DiffReferences.Except(Whitelist.Split(',')));

            return Task.FromResult(Unit.Default);
        }

        private async Task<IEnumerable<string>> LoadProjects(IReferenceAnalyzer analyzer) =>
            await analyzer.Load(Path, _tokenSource.Token);

        private async Task AnalyzeReferences(IReferenceAnalyzer analyzer,
            IObserver<ReferencesReport> observer,
            IEnumerable<string> projects)
        {
            await foreach (var element in analyzer.Analyze(projects, _tokenSource.Token))
                observer.OnNext(element);
            observer.OnCompleted();
        }
    }
}
