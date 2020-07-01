using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Moq;
using ReactiveUI;
using ReactiveUI.Testing;
using ReferenceExplorer.Models;
using ReferenceExplorer.WPF;
using Xunit;
using Xunit.Sdk;

namespace ReferenceExplorer.Tests
{
    public class AppViewModelTests
    {
        private const string _Path = "samplePath";
        private Mock<ISettings> _SettingsMock;
        private Mock<ISolutionProjectsProvider> _providerMock;
        private AppViewModel _Sut;
        private List<Project> _Projects;
        private List<Reference> _ReferencedProjects;
        private TestScheduler _Scheduler;

        public AppViewModelTests() => new TestScheduler().With(scheduler =>
        {
	        _Scheduler = scheduler;
	        _providerMock = new Mock<ISolutionProjectsProvider>();
	        _ReferencedProjects = new List<Reference>
	        {
		        new Reference {UsagesInSource = 5, Project = null}

	        };

	        _Projects = new List<Project>
	        {
		        new Project {Path = "path1"},
		        new Project {Path = "path2"},
	        };
	        _providerMock.Setup(m => m.GetProjectsFrom(_Path, It.IsAny<IProgress<int>>()))
		        .Returns(_Projects.ToAsync());

	        _SettingsMock = new Mock<ISettings>();
	        _SettingsMock.Setup(x => x.SolutionPath).Returns(_Path);
	        _SettingsMock.SetupSet(x => x.SolutionPath = It.IsAny<string>()).Verifiable();

	        _Sut = new AppViewModel(_providerMock.Object, _SettingsMock.Object);
        });

        [Fact]
        public void Instantiates()
        {
            Action a = () => new AppViewModel(_providerMock.Object, _SettingsMock.Object);

            a.Should().NotThrow();
        }

        [Fact]
        public void NoPathButtonDisabled()
        {
            var canExecute = true;
            _Sut.Path = "";
            _Sut.Load.CanExecute.Subscribe(x => canExecute = x );
            canExecute.Should().Be(false);
        }

        [Fact]
        public void CorrectPathSetEnabled()
        {
            var canExecute = false;
            _Sut.Path = "C:/Path";
            _Sut.Load.CanExecute.Subscribe(x => canExecute = x);
            canExecute.Should().Be(true);
        }

        [Fact]
        public void LoadedServiceInvoked()
        {
            var path = _Path;
            _Sut.Path = path;
            _Sut.Load.Execute().Subscribe();

            _providerMock.Verify(x => x.GetProjectsFrom(path, It.IsAny<IProgress<int>>()));
        }

        [Fact]
        public void LoadedServiceListUpdated()
        {
            _Sut.Path = _Path;
            _Sut.Load.Execute().Subscribe();

            _Scheduler.AdvanceBy(3);

            _Sut.Projects.Should().NotBeNullOrEmpty();
            _Sut.Projects.Should().BeEquivalentTo(_Projects);
        }

        [Fact]
        public void LoadedProjectSelectedShowsReferenceList()
        {
	        _Sut.Load.Execute().Subscribe();

            _Scheduler.AdvanceBy(3);

            _Sut.SelectedProject = _Sut.Projects.First();

            _Sut.SelectedProject.FormalReferences.Should().NotBeNull();
        }

        [Fact]
        public void DefaultSolutionPathTakenFromSettings()
        {
            var expected = _Path;

            _Sut.Path.Should().Be(expected);
        }

        [Fact]
        public void ChangingPathSavedInSettings()
        {
            var newPath = "newPath";
            _Sut.Path = newPath;

            _SettingsMock.VerifySet(x => x.SolutionPath=newPath);
        }

        [Fact]
        public void ExceptionInProjectProviderIsCaught()
        {
	        _providerMock.Setup(m => m.GetProjectsFrom(It.IsAny<string>(), It.IsAny<IProgress<int>>()))
		        .Throws<InvalidOperationException>();

	        Exception caughtException = null;

	        _Sut.Path = "test";
	        _Sut.Load.ThrownExceptions.ObserveOn(_Scheduler).Subscribe(e => caughtException = e);

	        _Sut.Load.Execute().Subscribe();
            _Scheduler.AdvanceBy(30000);

            caughtException.Should().NotBeNull();
        }

        [Fact]
        public void ExceptionInProjectProviderDoesNotThrowInAppViewModel()
        {
	        _providerMock.Setup(m => m.GetProjectsFrom(It.IsAny<string>(), It.IsAny<IProgress<int>>()))
		        .Throws<InvalidOperationException>();

	        _Sut.Path = "test";

	        Action a = () =>
	        {
		        _Sut.Load.Execute().Subscribe();
		        _Scheduler.AdvanceBy(3);
	        };

	        a.Invoke();

	        a.Should().NotThrow();
        }
    }
}