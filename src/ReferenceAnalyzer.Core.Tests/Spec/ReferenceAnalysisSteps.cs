using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Moq;
using TechTalk.SpecFlow;

namespace ReferenceAnalyzer.Core.Tests
{
    [Binding]
    public class ReferenceAnalysisSteps
    {
	    private readonly ReferenceAnalyzer _sut;
	    private ReferencesReport _result;
        private string _sinkOutput;

	    public ReferenceAnalysisSteps()
        {
            var sinkMock = new Mock<IMessageSink>();
            sinkMock.Setup(m => m.Write(It.IsAny<string>()))
                .Callback<string>(m => _sinkOutput = _sinkOutput + m + "\n");

            _sut = new ReferenceAnalyzer(sinkMock.Object);
	    }

        [Given(@"I have a solution (.*)")]
        public void GivenIHaveASolution(string solution)
        {
            var samplesPath = Assembly.GetExecutingAssembly().CodeBase?.Split("src")[0] + "test_samples";
            var slnPath = samplesPath + "/" + solution + "/" + solution + ".sln";
            var path = new Uri(slnPath).AbsolutePath;
            _sut.Load(path).Wait();
        }

        [When(@"I run analysis for (.*)")]
        public void WhenIRunAnalysis(string target)
        {

            _result = _sut.Analyze(target).Result;
        }

        [Then(@"number of references to (.*) should be (.*)")]
        public void ThenNumberOfReferencesShouldBe(string target, int references)
        {
	        _result.ReferencesTo(target).Should().Be(references);
        }

        [Then(@"Referenced projects should be within defined references list")]
        public void ThenReferencedProjectsShouldBeWithinDefinedReferencesList()
        {
            _result.DefinedReferences.Should().Contain("Project2").And.Contain("Project3");
        }

        [Then(@"Only (.*) should be in actual references")]
        public void ThenOnlyShouldBeInActualReferences(string referenceName)
        {
            _result.ActualReferences.Select(r => r.Target).Should().Contain(referenceName);
        }

        [Then(@"(.*) should not be in actual references")]
        public void ThenShouldNotBeInActualReferences(string referenceName)
        {
            _result.ActualReferences.Select(r => r.Target).Should().NotContain(referenceName);
        }

        [Then(@"No diagnostics should be reported")]
        public void ThenNoDiagnosticsShouldBeReported()
        {
            _sinkOutput.ToLower().Should().NotContain("error");
        }

    }
}
