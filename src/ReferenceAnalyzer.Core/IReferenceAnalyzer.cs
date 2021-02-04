using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ReferenceAnalyzer.Core.Models;

namespace ReferenceAnalyzer.Core
{
    public interface IReferenceAnalyzer
    {
        IDictionary<string, string> BuildProperties { get; set; }
        bool ThrowOnCompilationFailures { get; set; }

        Task<ReferencesReport> Analyze(Project target, CancellationToken token = default);
        Task<IEnumerable<Project>> Load(string solution, CancellationToken token = default);
        IObservable<Analysis> Analyze(IEnumerable<Project> projects, CancellationToken token = default);
    }
}
