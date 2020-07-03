using ReferenceAnalyzer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReferenceAnalyzer.WPF.Utilities
{
    class FakeProjectProvider : IProjectProvider
    {
        public ReferencesReport GetReferences(string path)
        {
            return new ReferencesReport(new[] { "ref1", "ref2", "ref3" }, 
                new[] { new ActualReference("ref1", Enumerable.Empty<ReferenceOccurrence>()) });
        }
    }
}
