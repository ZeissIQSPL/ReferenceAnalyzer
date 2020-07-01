using System;
using System.IO;
using System.Reflection;
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
            var samplesPath = Assembly.GetExecutingAssembly().CodeBase.Split("src")[0] + "test_samples";
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
    }
}
