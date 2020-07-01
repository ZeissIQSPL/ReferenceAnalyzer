using System;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace ReferenceAnalyzer.Core.Tests
{
    [Binding]
    public class ReferenceAnalysisSteps
    {
	    private readonly ReferenceAnalyzer _sut;
	    private ReferencesReport _result;

	    public ReferenceAnalysisSteps()
	    {
		    _sut = new ReferenceAnalyzer();
	    }

        [Given(@"I have a solution (.*)")]
        public void GivenIHaveASolution(string solution)
        {
	        _sut.Load(solution);
        }

        [When(@"I run analysis for (.*)")]
        public void WhenIRunAnalysis(string target)
        {
	        _result = _sut.Analyze(target);
        }

        [Then(@"number of references to (.*) should be (.*)")]
        public void ThenNumberOfReferencesShouldBe(string target, int references)
        {
	        _result.ReferencesTo(target).Should().Be(references);
        }
    }
}
