using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using ReferenceAnalyzer.Core.Models;
using ReferenceAnalyzer.Core.ProjectEdit;
using ReferenceAnalyzer.Core.Util;
using TechTalk.SpecFlow;

namespace ReferenceAnalyzer.Core.Tests
{
    [Binding]
    public class ReferenceAnalysisSteps
    {
        private readonly ReferenceAnalyzer _sut;
        private IEnumerable<Analysis> _manyResults;
        private ReferencesReport _result;
        private string _sinkOutput;
        private IEnumerable<Project> _loadedProjects;

        public ReferenceAnalysisSteps()
        {
            var sinkMock = new Mock<IMessageSink>();
            sinkMock.Setup(m => m.Write(It.IsAny<string>()))
                .Callback<string>(m => _sinkOutput = _sinkOutput + m + "\n");

            var editor = new Mock<IReferencesEditor>();

            editor.Setup(m => m.GetReferencedProjects(It.IsAny<string>()))
                .Returns(new [] {"Project2", "Project3"});

            var xamlReaderMock = new Mock<IXamlReferencesReader>();

            _sut = new ReferenceAnalyzer(sinkMock.Object, editor.Object, xamlReaderMock.Object);
        }

        [Given(@"I have a solution (.*)")]
        public void GivenIHaveASolution(string solution)
        {
            var samplesPath = GetTestSamplesLocation();
            var slnPath = samplesPath + "/" + solution + "/" + solution + ".sln";
            var path = new Uri(slnPath).AbsolutePath;

            var process = Process.Start("dotnet.exe", "restore " + path);
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new Exception($"Could not restore, exit code {process.ExitCode}");

            _loadedProjects = _sut.Load(path).Result;
        }

        [Given(@"I Disable throwing on errors")]
        public void WhenIDisableThrowingOnErrors() => _sut.ThrowOnCompilationFailures = false;

        private static string GetTestSamplesLocation() =>
            Assembly.GetExecutingAssembly().Location?.Split("src")[0] + "test_samples";

        [When(@"I run analysis for (.*)")]
        public void WhenIRunAnalysis(string target) =>
            _result = _sut.Analyze(new Project(target, "anyPath"), CancellationToken.None).Result;

        [When(@"I run full analysis")]
        public async Task WhenIRunFullAnalysis()
        {
            var analysis = _sut.Analyze(_loadedProjects, CancellationToken.None);

            var enumerable = analysis.ToEnumerable().ToList();

            _manyResults = enumerable;
        }


        [Then(@"number of references to (.*) should be (.*)")]
        public void ThenNumberOfReferencesShouldBe(string target, int references) =>
            _result.ReferencesTo(target).Should().Be(references);

        [Then(@"Referenced projects should be within defined references list")]
        public void ThenReferencedProjectsShouldBeWithinDefinedReferencesList() =>
            _result.DefinedReferences.Select(r => r.Target).Should().Contain(new[] {"Project2", "Project3"});

        [Then(@"Only (.*) should be in actual references")]
        public void ThenOnlyShouldBeInActualReferences(string referenceName) =>
            _result.ActualReferences.Select(r => r.Target).Should().Contain(referenceName);

        [Then(@"(.*) should not be in actual references")]
        public void ThenShouldNotBeInActualReferences(string referenceName) =>
            _result.ActualReferences.Select(r => r.Target).Should().NotContain(referenceName);

        [Then(@"No diagnostics should be reported")]
        public void ThenNoDiagnosticsShouldBeReported() => _sinkOutput.ToLower().Should().NotContain("error");

        [Then(@"Reports for all three should be returned")]
        public void ThenReportsForAllThreeShouldBeReturned()
        {
            _manyResults.Should().Contain(r => r.Project.Name == "Project1");
            _manyResults.Should().Contain(r => r.Project.Name == "Project2");
            _manyResults.Should().Contain(r => r.Project.Name == "Project3");
        }
    }
}
