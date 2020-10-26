using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReferenceAnalyzer.Core
{
    public interface IReferenceAnalyzer
    {
        IDictionary<string, string> BuildProperties { get; set; }
        bool ThrowOnCompilationFailures { get; set; }
        IProgress<double> ProgressReporter { get; set; }

        IAsyncEnumerable<ReferencesReport> AnalyzeAll(string solutionPath, CancellationToken token = default);
        Task<ReferencesReport> Analyze(string target, CancellationToken token = default);
        Task<IEnumerable<string>> Load(string solution, CancellationToken token = default);
        IAsyncEnumerable<ReferencesReport> AnalyzeAll(CancellationToken token = default);
        IAsyncEnumerable<ReferencesReport> Analyze(IEnumerable<string> projects, CancellationToken token = default);
    }
}
