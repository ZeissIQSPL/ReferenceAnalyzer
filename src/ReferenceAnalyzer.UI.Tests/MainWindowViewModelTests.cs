using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Moq;
using ReactiveUI.Testing;
using ReferenceAnalyzer.UI.Models;
using ReferenceAnalyzer.Core;
using ReferenceAnalyzer.Core.ProjectEdit;
using ReferenceAnalyzer.Core.Util;
using ReferenceAnalyzer.UI.Services;
using ReferenceAnalyzer.UI.ViewModels;
using Xunit;

namespace ReferenceAnalyzer.UI.Tests
{
    public class MainWindowViewModelTests
    {
        private const string Path = "samplePath";
        private Mock<ISettings> _settingsMock;
        private Mock<IReferenceAnalyzer> _analyzerMock;
        private MainWindowViewModel _sut;
        private IEnumerable<ReferencesReport> _reports;
        private TestScheduler _scheduler;
        private string _receivedPopupMessage;
        private Mock<IReferencesEditor> _editor;
        private Mock<IReadableMessageSink> _sinkMock;

        public MainWindowViewModelTests()
        {
            SetupAnalyzer();

            SetupSettings();

            new TestScheduler().With(scheduler =>
            {
                _editor = new Mock<IReferencesEditor>();
                _sinkMock = new Mock<IReadableMessageSink>();
                _sinkMock.Setup(m => m.Lines)
                    .Returns(new ReadOnlyObservableCollection<string>(new ObservableCollection<string>()));

                _sut = new MainWindowViewModel(_settingsMock.Object, _analyzerMock.Object, _editor.Object, _sinkMock.Object);

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
                new ActualReference("project1", Enumerable.Empty<ReferenceOccurrence>())
            };

            _analyzerMock.Setup(m => m.Load(It.IsAny<string>()))
                .Returns(Task.FromResult(new[] {"proj1", "proj2"}.AsEnumerable()));

            var firstReport = new ReferencesReport("proj1", new[] {"ref1", "ref2"}, referencedProjects, "anyPath");
            var secondReport = new ReferencesReport("proj2", new[] {"ref2", "ref3"}, Enumerable.Empty<ActualReference>(), "anyPath");

            _reports = new[] {firstReport, secondReport};

            _analyzerMock.Setup(m => m.Analyze("proj1"))
                .Returns(Task.FromResult(firstReport));

            _analyzerMock.Setup(m => m.Analyze("proj2"))
                .Returns(Task.FromResult(secondReport));

            _analyzerMock.Setup(m => m.Analyze(It.IsAny<IEnumerable<string>>()))
                .Returns(_reports.ToAsync());

            _analyzerMock.Setup(m => m.Load("error_project"))
                .Throws<InvalidOperationException>();
        }

        private void SetupSettings()
        {
            _settingsMock = new Mock<ISettings>();
            _settingsMock.Setup(x => x.SolutionPath).Returns(Path);
            _settingsMock.SetupSet(x => x.SolutionPath = It.IsAny<string>()).Verifiable();
        }

        [Fact]
        public void Instantiates()
        {
            Action a = () => _ = new MainWindowViewModel(_settingsMock.Object, _analyzerMock.Object, _editor.Object, _sinkMock.Object);

            a.Should().NotThrow();
        }

        [Fact]
        public void DefaultSolutionPathTakenFromSettings()
        {
            const string expected = Path;

            _sut.Path.Should().Be(expected);
        }

        [Fact]
        public void ChangingPathSavedInSettings()
        {
            const string newPath = "newPath";
            _sut.Path = newPath;

            _settingsMock.VerifySet(x => x.SolutionPath = newPath);
        }

        [Fact]
        public void CorrectPathSetLoadingEnabled()
        {
            var canExecute = false;
            _sut.Path = "C:/Path";
            _sut.Load.CanExecute.Subscribe(x => canExecute = x);
            canExecute.Should().Be(true);
        }

        [Fact]
        public void NoPathButtonDisabled()
        {
            var canExecute = true;
            _sut.Path = "";
            _sut.Load.CanExecute.Subscribe(x => canExecute = x);
            canExecute.Should().Be(false);
        }

        [Fact]
        public void ExceptionThrownInsideLoadingCommand()
        {
            _sut.Path = "error_project";

            var wasError = false;
            _sut.Load.ThrownExceptions.Subscribe(_ => wasError = true);

            _sut.Load.Execute().Subscribe(_ => { }, onError: _ => { });
            _scheduler.AdvanceBy(3);

            wasError.Should().BeTrue();
        }

        [Fact]
        public void ExceptionInLoadingMessageSentToInteraction()
        {
            _sut.Path = "error_project";

            _sut.Load.Execute().Subscribe(_ => { }, onError: _ => {});
            _scheduler.AdvanceBy(3);

            _receivedPopupMessage.Should().NotBeNull();
        }

        [Fact]
        public void LoadedProjectSelectedShowsReferenceList()
        {
            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.Analyze.Execute(new [] {"proj1"}).Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.SelectedProject = _sut.Reports.First().Project;

            _sut.SelectedProjectReport.DefinedReferences.Should().NotBeNull();
        }

        [Fact]
        public void LoadedServiceInvoked()
        {
            var path = Path;
            _sut.Path = path;
            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _analyzerMock.Verify(x => x.Load(path));
        }

        [Fact]
        public void LoadedServiceListUpdated()
        {
            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.Analyze.Execute(new[] {"proj1"}).Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.SelectedProject = _sut.Reports.First().Project;

            _sut.SelectedProjectReport.Should().NotBeNull();
            _sut.SelectedProjectReport.Should().Be(_reports.First());
        }

        [Fact]
        public void AnalyzeAllDisabledIfNoSolutionLoaded()
        {
            var canExecute = true;
            _sut.Path = "";
            _sut.Analyze.CanExecute.Subscribe(x => canExecute = x);
            canExecute.Should().Be(false);
        }

        [Fact]
        public void AnalyzeAllEnabledAfterSolutionLoaded()
        {
            var canExecute = false;
            _sut.Path = "any";
            _sut.Analyze.CanExecute.Subscribe(x => canExecute = x);

            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            canExecute.Should().Be(true);
        }

        [Fact]
        public void AnalyzeSelectedNotEnabledIfNoProjectSelected()
        {
            var canExecute = false;
            _sut.Path = "any";
            _sut.AnalyzeSelected.CanExecute.Subscribe(x => canExecute = x);

            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            canExecute.Should().Be(false);
        }

        [Fact]
        public void AnalyzeSelectedEnabledIfAnyProjectSelected()
        {
            var canExecute = false;
            _sut.Path = "any";
            _sut.AnalyzeSelected.CanExecute.Subscribe(x => canExecute = x);

            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.SelectedProject = _sut.Projects.First();

            canExecute.Should().Be(true);
        }

        [Fact]
        public void ProjectsListIsClearedBetweenLoads()
        {
            _sut.Path = "any";
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
            _sut.Path = "any";
            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.Analyze.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.Reports.Should().NotBeEmpty();

            var firstCount = _sut.Projects.Count;
            _sut.Analyze.Execute().Subscribe();
            _scheduler.AdvanceBy(4);

            _sut.Reports.Count.Should().Be(firstCount);
        }

        [Fact]
        public void SelectingNotAnalyzedProjectEmptyReportReturned()
        {
            _sut.Path = "any";
            _sut.Load.Execute().Subscribe();
            _scheduler.AdvanceBy(3);

            _sut.SelectedProject = _sut.Projects.First();

            _sut.SelectedProjectReport.DefinedReferences.Should().BeEmpty();
            _sut.SelectedProjectReport.ActualReferences.Should().BeEmpty();
        }
    }
}
