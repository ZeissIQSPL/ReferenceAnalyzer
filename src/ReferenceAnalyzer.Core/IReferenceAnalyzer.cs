using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReferenceAnalyzer.Core
{
    public interface IReferenceAnalyzer
    {
        IDictionary<string, string> BuildProperties { get; set; }
        bool ThrowOnCompilationFailures { get; set; }
        IAsyncEnumerable<ReferencesReport> AnalyzeAll(string solutionPath);
        Task<ReferencesReport> Analyze(string target);
    }
}
