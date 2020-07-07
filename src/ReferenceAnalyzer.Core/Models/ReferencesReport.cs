using System.Collections.Generic;
using System.Linq;

namespace ReferenceAnalyzer.Core
{
    public class ReferencesReport
    {
        public ReferencesReport(string project, IEnumerable<string> definedReferences,
            IEnumerable<ActualReference> actualReferences)
        {
            Project = project;
            DefinedReferences = definedReferences;
            ActualReferences = actualReferences;
        }

        public IEnumerable<ActualReference> ActualReferences { get; }
        public string Project { get; }
        public IEnumerable<string> DefinedReferences { get; }

        public IEnumerable<string> DiffReferences => ActualReferences
            .Select(x => x.Target)
            .Except(DefinedReferences);


        public int ReferencesTo(string target)
        {
            var reference = ActualReferences
                .FirstOrDefault(r => r.Target == target);
            return reference == null ? 0 : reference.Occurrences.Count();
        }

        public override string ToString() => Project;
    }
}
