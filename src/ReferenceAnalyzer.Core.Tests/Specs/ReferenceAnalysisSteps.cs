using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using ReferenceAnalyzer.Core.ProjectEdit;
using TechTalk.SpecFlow;

namespace ReferenceAnalyzer.Core.Tests
{
    [Binding]
    public class ReferenceAnalysisSteps
    {
        private readonly ReferenceAnalyzer _sut;
        private IEnumerable<ReferencesReport> _manyResults;
        private ReferencesReport _result;
        private string _sinkOutput;
        private Mock<IProgress<double>> _progress;
        private double _lastProgress;

        public ReferenceAnalysisSteps()
        {
            var sinkMock = new Mock<IMessageSink>();
            sinkMock.Setup(m => m.Write(It.IsAny<string>()))
                .Callback<string>(m => _sinkOutput = _sinkOutput + m + "\n");

            var editor = new Mock<IReferencesEditor>();

            editor.Setup(m => m.GetReferencedProjects(It.IsAny<string>()))
                .Returns(new [] {"Project2", "Project3"});

            _sut = new ReferenceAnalyzer(sinkMock.Object, editor.Object);
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

            _sut.Load(path).Wait();
        }

        [Given(@"I Disable throwing on errors")]
        public void WhenIDisableThrowingOnErrors() => _sut.ThrowOnCompilationFailures = false;

        [Given(@"I setup progress tracking")]
        public void GivenISetupProgressTracking()
        {
            _progress = new Mock<IProgress<double>>();
            _progress.Setup(m => m.Report(It.IsAny<double>()))
                .Callback((double v) => _lastProgress = v);
            _sut.ProgressReporter = _progress.Object;
        }

        [Given(@"I enable NuGet analysis")]
        public void GivenIEnableNuGetAnalysis() => true;


        private static string GetTestSamplesLocation() =>
            Assembly.GetExecutingAssembly().CodeBase?.Split("src")[0] + "test_samples";

        [When(@"I run analysis for (.*)")]
        public void WhenIRunAnalysis(string target) => _result = _sut.Analyze(target).Result;

        [When(@"I run full analysis")]
        public async Task WhenIRunFullAnalysis()
        {
            var enumerable = _sut.AnalyzeAll();
            var results = new List<ReferencesReport>();
            await foreach (var result in enumerable)
                results.Add(result);

            _manyResults = results;
        }


        [Then(@"number of references to (.*) should be (.*)")]
        public void ThenNumberOfReferencesShouldBe(string target, int references) =>
            _result.ReferencesTo(target).Should().Be(references);

        [Then(@"Referenced projects should be within defined references list")]
        public void ThenReferencedProjectsShouldBeWithinDefinedReferencesList() =>
            _result.DefinedReferences.Should().Contain(new[] {"Project2", "Project3"});

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
            _manyResults.Should().Contain(r => r.Project == "Project1");
            _manyResults.Should().Contain(r => r.Project == "Project2");
            _manyResults.Should().Contain(r => r.Project == "Project3");
        }

        [Then(@"I should receive progress report")]
        public void ThenIShouldReceiveProgressReport()
        {
            _progress.Verify(m => m.Report(It.IsAny<double>()), Times.AtLeastOnce);
        }

        [Then(@"Last progress report should be complete")]
        public void ThenLastProgressReportShouldBeComplete()
        {
            _lastProgress.Should().Be(1);
        }

    }
}
