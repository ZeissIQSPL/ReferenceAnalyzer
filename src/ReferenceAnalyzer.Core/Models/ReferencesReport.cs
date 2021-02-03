using System;
using System.Collections.Generic;
using System.Linq;

namespace ReferenceAnalyzer.Core.Models
{
    public record ReferencesReport
    {
        public ReferencesReport(IEnumerable<Reference> definedReferences,
            IEnumerable<ActualReference> actualReferences)
        {
            DefinedReferences = definedReferences;
            ActualReferences = actualReferences;
        }

        public IEnumerable<ActualReference> ActualReferences { get; }
        public IEnumerable<Reference> DefinedReferences { get; }

        public IEnumerable<Reference> DiffReferences => DefinedReferences
            .Except(ActualReferences);

        public static ReferencesReport Empty =>
            new(Enumerable.Empty<Reference>(),
                Enumerable.Empty<ActualReference>());
    }
}
