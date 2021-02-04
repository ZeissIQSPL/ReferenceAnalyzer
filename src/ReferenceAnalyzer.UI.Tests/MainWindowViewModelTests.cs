using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Moq;
using ReactiveUI.Testing;
using ReferenceAnalyzer.Core;
using ReferenceAnalyzer.Core.ProjectEdit;
using ReferenceAnalyzer.Core.Util;
using ReferenceAnalyzer.UI.Services;
using ReferenceAnalyzer.UI.ViewModels;
using Xunit;
using System.Threading;
using ReferenceAnalyzer.Core.Models;

namespace ReferenceAnalyzer.UI.Tests
{
    public class MainWindowViewModelTests
    {
        private const string Path = "samplePath";
        private Mock<IReferenceAnalyzer> _analyzerMock;
        private MainWindowViewModel _sut;
        private IEnumerable<Analysis> _reports;
        private TestScheduler _scheduler;
        private string _receivedPopupMessage;
        private Mock<IReferencesEditor> _editor;
        private Mock<IReadableMessageSink> _sinkMock;
        private Mock<ISolutionViewModel> _solutionViewModel;
        private Project _project1;
        private Project _project2;

        public MainWindowViewModelTests()
        {
            SetupAnalyzer();

            new TestScheduler().With(scheduler =>
            {
                _editor = new Mock<IReferencesEditor>();
                _sinkMock = new Mock<IReadableMessageSink>();
                _solutionViewModel = new Mock<ISolutionViewModel>();
                _sinkMock.Setup(m => m.Lines)
                    .Returns(new ReadOnlyObservableCollection<string>(new ObservableCollection<string>()));

                _sut = new MainWindowViewModel(_solutionViewModel.Object, _analyzerMock.Object,
                    _editor.Object, _sinkMock.Object);

                _receivedPopupMessage = null;
                _sut.MessagePopup
                    .RegisterHandler(interaction =>
                    {
                        _receivedPopupMessage = interaction.Input;
                        interaction.SetOutput(Unit.Default);
                    });
                _scheduler = scheduler;
            });
        }

        private void SetupAnalyzer()
        {
            _analyzerMock = new Mock<IReferenceAnalyzer>();
            var referencedProjects = new List<ActualReference>
            {
                new("project1", Enumerable.Empty<ReferenceOccurrence>())
            };

            _project1 = new Project("proj1", "path1");
            _project2 = new Project("proj2", "path2");
            _analyzerMock.Setup(m => m.Load(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new[] {_project1, _project2}.AsEnumerable()));

            var firstReport = new ReferencesReport(new[] {new Reference("ref1"), new Reference("ref2")}, referencedProjects);
            var secondReport = new ReferencesReport(new[] {new Reference("ref2"), new Reference("ref3")}, Enumerable.Empty<ActualReference>());

            _reports = new[] {new Analysis(_project1, Observable.Return(firstReport)), new Analysis(_project2, Observable.Return(secondReport))};

            _analyzerMock.Setup(m => m.Analyze(_project1, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(firstReport));

            _analyzerMock.Setup(m => m.Analyze(_project2, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(secondReport));

            _analyzerMock.Setup(m => m.Analyze(It.IsAny<IEnumerable<Project>>(), It.IsAny<CancellationToken>()))
                .Returns(_reports.ToObservable());

            _analyzerMock.Setup(m => m.Load("error_project", It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>();
        }


        [Fact]
        public void Instantiates()
        {
            Action a = () => _ = new MainWindowViewModel(_solutionViewModel.Object,
                _analyzerMock.Object, _editor.Object, _sinkMock.Object);

            a.Should().NotThrow();
        }

        [Fact]
        public void CorrectPathSetLoadingEnabled()
        {
            var canExecute = false;
            _solutionViewModel.Setup(x => x.Path).Returns("C:/Path");
            _sut.Load.CanExecute.Subscribe(x => canExecute = x);
            _scheduler.AdvanceBy(3);
            canExecute.Should().Be(true);
        }

        [Fact]
        public void NoPathButtonDisabled()
        {
            var canExecute = true;
            _solutionViewModel.Setup(x => x.Path).Returns("");
            _sut.Load.CanExecute.Subscribe(x => canExecute = x);
            canExecute.Should().Be(false);
        }

        [Fact]
        public void ExceptionThrownInsideLoadingCommand()
        {
            _solutionViewModel.Setup(x => x.Path).Returns("error_project");
            var wasError = false;
            _sut.Load.ThrownExceptions.Subscribe(_ => wasError = true);

            _sut.Load.Execute().Subscribe(_ => { }, onError: _ => { });
            _scheduler.AdvanceBy(3);

            wasError.Should().BeTrue();
        }

        [Fact]
        public void ExceptionInLoadingMessageSentToInteraction()
        {
            _solutionViewModel.Setup(x => x.Path).Returns("error_project");

            _sut.Load.Execute().Subscribe(_ => { }, onError: _ => {});
            _scheduler.AdvanceBy(3);

            _receivedPopupMessage.Should().NotBeNull();
        }

        [Fact]
        public void LoadedProjectSelectedShowsReferenceList()
        {
            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.Analyze.Execute(new [] {_project1}).Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.SelectedProject = _sut.Projects.First();

            _sut.SelectedProjectReport.DefinedReferences.Should().NotBeNull();
        }

        [Fact]
        public void LoadedServiceInvoked()
        {
            var path = Path;

            _solutionViewModel.Setup(x => x.Path).Returns(Path);
            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _analyzerMock.Verify(x => x.Load(path, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async void LoadedServiceListUpdated()
        {
            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.Analyze.Execute(new [] {_project1}).Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.SelectedProject = _sut.Projects.First();

            _sut.SelectedProject.Should().NotBeNull();
            _sut.SelectedProject.Should().Be(_reports.First().Project);
        }

        [Fact]
        public void AnalyzeAllDisabledIfNoSolutionLoaded()
        {
            var canExecute = true;
            _solutionViewModel.Setup(x => x.Path).Returns("");
            _sut.Analyze.CanExecute.Subscribe(x => canExecute = x);
            canExecute.Should().Be(false);
        }

        [Fact]
        public void AnalyzeAllEnabledAfterSolutionLoaded()
        {
            var canExecute = false;
            _solutionViewModel.Setup(x => x.Path).Returns("any");
            _sut.Analyze.CanExecute.Subscribe(x => canExecute = x);

            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            canExecute.Should().Be(true);
        }

        [Fact]
        public void AnalyzeSelectedNotEnabledIfNoProjectSelected()
        {
            var canExecute = false;
            _solutionViewModel.Setup(x => x.Path).Returns("any");
            _sut.AnalyzeSelected.CanExecute.Subscribe(x => canExecute = x);

            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            canExecute.Should().Be(false);
        }

        [Fact]
        public void AnalyzeSelectedEnabledIfAnyProjectSelected()
        {
            var canExecute = false;
            _solutionViewModel.Setup(x => x.Path).Returns("any");
            _sut.AnalyzeSelected.CanExecute.Subscribe(x => canExecute = x);

            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.SelectedProject = _sut.Projects.First();

            canExecute.Should().Be(true);
        }

        [Fact]
        public void ProjectsListIsClearedBetweenLoads()
        {
            _solutionViewModel.Setup(x => x.Path).Returns("any");
            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.Projects.Should().NotBeEmpty();

            var firstCount = _sut.Projects.Count;
            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.Projects.Count.Should().Be(firstCount);
        }

        [Fact]
        public void ReportsAreClearedBetweenLoads()
        {
            _solutionViewModel.Setup(x => x.Path).Returns("any");
            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.Analyze.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.Projects.Should().NotBeEmpty();

            var firstCount = _sut.Projects.Count;
            _sut.Analyze.Execute().Subscribe();
            _scheduler.AdvanceBy(4);

            _sut.Projects.Count.Should().Be(firstCount);
        }

        [Fact]
        public void SelectingNotAnalyzedProjectEmptyReportReturned()
        {
            _solutionViewModel.Setup(x => x.Path).Returns("any");
            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.SelectedProject = _sut.Projects.First();

            _sut.SelectedProjectReport.DefinedReferences.Should().BeEmpty();
            _sut.SelectedProjectReport.ActualReferences.Should().BeEmpty();
        }
    }
}
