using System;

namespace ReferenceAnalyzer.Core.Models
{
    public record Analysis(Project Project, IObservable<ReferencesReport> Report);
}
