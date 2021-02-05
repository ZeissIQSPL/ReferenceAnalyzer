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
using ReferenceAnalyzer.Core.Models;
using ReferenceAnalyzer.Core.ProjectEdit;
using ReferenceAnalyzer.UI.Services;

namespace ReferenceAnalyzer.UI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private ReadOnlyObservableCollection<Project> _projects;
        private bool _stopOnError;
        private Project _selectedProject = Project.Empty;
        private string _log;
        private string _whitelist;
        private CancellationTokenSource _tokenSource;
        private ObservableAsPropertyHelper<double> _progress;


        public MainWindowViewModel(ISolutionViewModel solutionViewModel, IReferenceAnalyzer analyzer, IReferencesEditor editor,
                IReadableMessageSink messageSink)
        {
            if (analyzer == null)
                throw new ArgumentNullException(nameof(analyzer));
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));
            if (messageSink == null)
                throw new ArgumentNullException(nameof(messageSink));


            SolutionViewModel = solutionViewModel;
            _tokenSource = new CancellationTokenSource();

            ConfigureAnalyzer(analyzer);
            SetupCommands(analyzer, editor);
            SetupProperties(analyzer);
            SetupSink(messageSink);
        }

        public IReadableMessageSink MessageSink { get; set; }

        public ReadOnlyObservableCollection<Project> Projects => _projects;

        public ReactiveCommand<Unit, IEnumerable<Project>> Load { get; private set; }
        public ReactiveCommand<Unit, Unit> Cancel { get; private set; }
        public ReactiveCommand<IEnumerable<Project>, Analysis> Analyze { get; private set; }
        public ReactiveCommand<Project, Analysis> AnalyzeSelected { get; set; }
        public ReactiveCommand<Project, Unit> RemoveUnused { get; private set; }
        public ReactiveCommand<IEnumerable<Project>, Unit> RemoveAllUnused { get; set; }

        public Interaction<string, Unit> MessagePopup { get; } = new();

        public ISolutionViewModel SolutionViewModel { get; }

        public ReferencesReport SelectedProjectReport => SelectedProject?.Report ?? ReferencesReport.Empty;

        public Project SelectedProject
        {
            get => _selectedProject;
            set => this.RaiseAndSetIfChanged(ref _selectedProject, value);
        }

        public bool StopOnError
        {
            get => _stopOnError;
            set => this.RaiseAndSetIfChanged(ref _stopOnError, value);
        }

        public double Progress => _progress.Value;

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
        }

        private void SetupSink(IReadableMessageSink messageSink)
        {
            MessageSink = messageSink;

            MessageSink.Lines.ToObservableChangeSet()
                .Select(_ => MessageSink.Lines)
                .Subscribe(lines => Log = string.Join('\n', lines));
        }

        private void SetupProperties(IReferenceAnalyzer analyzer)
        {

            this.WhenAnyValue(viewModel => viewModel.StopOnError)
                .Subscribe(x => analyzer.ThrowOnCompilationFailures = x);

            this.WhenAnyValue(viewModel => viewModel.SelectedProject, viewModel => viewModel.Projects.Count)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(SelectedProjectReport)));
        }

        private void SetupCommands(IReferenceAnalyzer projectProvider,
            IReferencesEditor editor)
        {

            Cancel = ReactiveCommand.Create(() =>
            {
                _tokenSource.Cancel();
                _tokenSource = new CancellationTokenSource();
            });

            SetupLoad(projectProvider);
            SetupRemove(editor);
        }

        private void SetupLoad(IReferenceAnalyzer analyzer)
        {
            var canLoad = this.WhenAnyValue(x => x.SolutionViewModel.Path,
                path => !string.IsNullOrEmpty(path));

            Load = ReactiveCommand.CreateFromTask(() =>
                    LoadProjects(analyzer),
                canLoad);

            Load.ThrownExceptions
                .Subscribe(HandleCommandExceptions);

            var projects = new SourceList<Project>();

            Load.Subscribe(p => projects.AddRange(p));
            Load.IsExecuting
                .Where(executionStarted => executionStarted)
                .Subscribe(_ => projects.Clear());

            projects.Connect()
                .Bind(out _projects)
                .Subscribe();

            //analysis
            var canAnalyze = Projects.ToObservableChangeSet()
                .Select(_ => Projects?.Any() == true);

            Analyze = ReactiveCommand.CreateFromObservable<IEnumerable<Project>, Analysis>(projects =>
                    AnalyzeReferences(analyzer, projects), canAnalyze);

            Analyze.Subscribe(analysis =>
            {
                projects.Replace(projects.Items.First(p => p.Path == analysis.Project.Path),
                    analysis.Project with {AnalysisStage = EAnalysisStage.InProgress});

                analysis.Report.Subscribe(report =>
                {
                    var project = projects.Items.First(p => p.Path == analysis.Project.Path);
                    projects.Replace(project,
                        project with { Report = report, AnalysisStage = EAnalysisStage.Finished });
                });
            });

            Analyze.ThrownExceptions
                .Subscribe(HandleCommandExceptions);

            var analyzeProgress = Analyze
                .Scan(0, (acc, _) => acc + 1)
                .Select(c => (double)c / _projects.Count)
                .Merge(Analyze.IsExecuting.Select(x => x ? 0.0 : 1.0));
            var loadProgress = Load.IsExecuting
                .Select(x => x ? -1.0 : 1.0);

            _progress = analyzeProgress
                .Merge(loadProgress)
                .ToProperty(this, vm => vm.Progress);

            var canAnalyzeSelected = this.WhenAnyValue(vm => vm.SelectedProject)
                .Select(p => p is not null && p != Project.Empty);

            AnalyzeSelected = ReactiveCommand.CreateFromObservable<Project, Analysis>(p =>
                Analyze.Execute(new[] { p }), canAnalyzeSelected);
        }

        private async void HandleCommandExceptions(Exception error)
        {
            if (error is OperationCanceledException)
                return;

            await MessagePopup.Handle(error.Message);
        }

        private void SetupRemove(IReferencesEditor editor)
        {
            var canRemove = this.WhenAnyValue(x => x.SelectedProjectReport,
                report => report.DiffReferences.Any());

            RemoveUnused = ReactiveCommand.CreateFromTask<Project, Unit>(project =>
                    RemoveReferences(project, editor),
                canRemove);

            RemoveAllUnused = ReactiveCommand.CreateFromTask<IEnumerable<Project>, Unit>(async projects =>
                {
                    await Task.WhenAll(projects.Select(project => RemoveReferences(project, editor)));
                    return Unit.Default;
                },
                this.WhenAnyValue(x => x.Projects.Count, count => count > 0));
        }

        private Task<Unit> RemoveReferences(Project project, IReferencesEditor editor)
        {
            editor.RemoveReferencedProjects(project.Path,
                project.Report.DiffReferences
                    .Select(r => r.Target)
                    .Except(Whitelist.Split(',')));

            return Task.FromResult(Unit.Default);
        }

        private async Task<IEnumerable<Project>> LoadProjects(IReferenceAnalyzer analyzer) =>
            await analyzer.Load(SolutionViewModel.Path);

        private IObservable<Analysis> AnalyzeReferences(IReferenceAnalyzer analyzer, IEnumerable<Project> projects)
        {
            return analyzer.Analyze(projects, _tokenSource.Token)
                .SubscribeOn(RxApp.TaskpoolScheduler);
        }
    }

    public class ProjectViewModel
    {
        public ProjectViewModel(string name, EAnalysisStage stage)
        {
            Name = name;
            Stage = stage;
        }

        public EAnalysisStage Stage { get; set; }

        public string Name { get; }
    }
}
