namespace ReferenceAnalyzer.Core.Models
{
    public record Project(string Name, string Path, ReferencesReport? Report = null, EAnalysisStage AnalysisStage = EAnalysisStage.NotStarted)
    {
        public ReferencesReport Report { get; set; } = Report ?? ReferencesReport.Empty;

        public static Project Empty => new("", "", ReferencesReport.Empty);
    }
}
