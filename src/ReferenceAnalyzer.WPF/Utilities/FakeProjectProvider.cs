using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReferenceAnalyzer.Core;

namespace ReferenceAnalyzer.WPF.Utilities
{
    internal class FakeProjectProvider : IReferenceAnalyzer
    {
        public IDictionary<string, string> BuildProperties { get; set; }
        public bool ThrowOnCompilationFailures { get; set; }

        public async IAsyncEnumerable<ReferencesReport> AnalyzeAll(string solutionPath)
        {
            yield return await Task.FromResult(
                new ReferencesReport("Project 1", new[] {"ref1", "ref2", "ref3"},
                    new[]
                    {
                        new ActualReference("ref1", Enumerable.Empty<ReferenceOccurrence>()),
                        new ActualReference("ref5", Enumerable.Empty<ReferenceOccurrence>())
                    }
                ));
        }

        public Task<ReferencesReport> Analyze(string target) => throw new System.NotImplementedException();
        public async Task Load(string solution) => throw new System.NotImplementedException();
    }
}
