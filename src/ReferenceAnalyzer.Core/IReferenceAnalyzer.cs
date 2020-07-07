using System.Collections.Generic;

namespace ReferenceAnalyzer.Core
{
    public interface IReferenceAnalyzer
    {
        IDictionary<string, string> BuildProperties { get; set; }
        IAsyncEnumerable<ReferencesReport> AnalyzeAll(string solutionPath);
    }
}
