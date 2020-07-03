using ReferenceAnalyzer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceAnalyzer.WPF.Utilities
{
    class FakeProjectProvider : IReferenceAnalyzer
    {
        public IDictionary<string, string> BuildProperties { get; set; }

        public async IAsyncEnumerable<ReferencesReport> AnalyzeAll(string solutionPath)
        {
            yield return await Task.FromResult(
                new ReferencesReport("Project 1", new[] { "ref1", "ref2", "ref3" },
                  new[] { 
                      new ActualReference("ref1", Enumerable.Empty<ReferenceOccurrence>()), 
                      new ActualReference("ref5", Enumerable.Empty<ReferenceOccurrence>()) 
                  }
              ));

        }


    }
}
