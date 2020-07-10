using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Moq;
using ReactiveUI.Testing;
using ReferenceAnalyzer.Core;
using ReferenceAnalyzer.Core.Util;
using ReferenceAnalyzer.WPF.Utilities;
using Xunit;

namespace ReferenceAnalyzer.WPF.Tests
{
    public class AppViewModelTests
    {
        public AppViewModelTests()
        {
            _providerMock = new Mock<IReferenceAnalyzer>();
            var referencedProjects = new List<ActualReference>
            {
                new ActualReference("project1", Enumerable.Empty<ReferenceOccurrence>())
            };

            _projects = new List<ReferencesReport>
            {
                new ReferencesReport("proj1", new[] {"ref1", "ref2"}, referencedProjects),
                new ReferencesReport("proj2", new[] {"ref2", "ref3"}, null)
            };
            _providerMock.Setup(m => m.AnalyzeAll(Path))
                .Returns(_projects.ToAsync());

            _settingsMock = new Mock<ISettings>();
            _settingsMock.Setup(x => x.SolutionPath).Returns(Path);
            _settingsMock.SetupSet(x => x.SolutionPath = It.IsAny<string>()).Verifiable();

            _sut = new AppViewModel(_settingsMock.Object, _providerMock.Object);
        }

        private const string Path = "samplePath";
        private readonly Mock<ISettings> _settingsMock;
        private readonly Mock<IReferenceAnalyzer> _providerMock;
        private readonly AppViewModel _sut;
        private readonly List<ReferencesReport> _projects;

        [Fact]
        public void ChangingPathSavedInSettings()
        {
            var newPath = "newPath";
            _sut.Path = newPath;

            _settingsMock.VerifySet(x => x.SolutionPath = newPath);
        }

        [Fact]
        public void CorrectPathSetEnabled()
        {
            var canExecute = false;
            _sut.Path = "C:/Path";
            _sut.Load.CanExecute.Subscribe(x => canExecute = x);
            canExecute.Should().Be(true);
        }

        [Fact]
        public void DefaultSolutionPathTakenFromSettings()
        {
            var expected = Path;

            _sut.Path.Should().Be(expected);
        }

        [Fact]
        public void ExceptionThrownInsideLoadingCommand()
        {
            _providerMock.Setup(m => m.AnalyzeAll(It.IsAny<string>())).Throws<InvalidOperationException>();

            _sut.Path = "any";

            var wasError = false;

            _sut.Load.ThrownExceptions.Subscribe(_ => wasError = true);

            _sut.Load.Execute().Subscribe();

            wasError.Should().BeTrue();
        }

        [Fact]
        public void Instantiates()
        {
            Action a = () => _ = new AppViewModel(_settingsMock.Object, _providerMock.Object);

            a.Should().NotThrow();
        }

        [Fact]
        public void LoadedProjectSelectedShowsReferenceList()
        {
            _sut.Load.Execute().Subscribe();

            _sut.SelectedProjectReport = _sut.Reports.First();

            _sut.SelectedProjectReport.DefinedReferences.Should().NotBeNull();
        }

        [Fact]
        public void LoadedServiceInvoked()
        {
            var path = Path;
            _sut.Path = path;
            _sut.Load.Execute().Subscribe();

            _providerMock.Verify(x => x.AnalyzeAll(path));
        }

        [Fact]
        public void LoadedServiceListUpdated() => new TestScheduler().With(scheduler =>
        {
            var sut = new AppViewModel(_settingsMock.Object, _providerMock.Object)
            {
                Path = Path
            };
            sut.Load.Execute().Subscribe();

            scheduler.AdvanceBy(3);

            sut.Reports.Should().NotBeNullOrEmpty();
            sut.Reports.Should().BeEquivalentTo(_projects);
        });

        [Fact]
        public void NoPathButtonDisabled()
        {
            var canExecute = true;
            _sut.Path = "";
            _sut.Load.CanExecute.Subscribe(x => canExecute = x);
            canExecute.Should().Be(false);
        }

        [Fact]
        public void AnalyzeAllDisabledIfNoSolutionLoaded()
        {
            var canExecute = true;
            _sut.Path = "";
            _sut.Analyze.CanExecute.Subscribe(x => canExecute = x);
            canExecute.Should().Be(false);
        }
    }
}
