using System.Collections.Generic;

namespace ReferenceAnalyzer.Core.Models
{
    public record ActualReference(string Target, IEnumerable<ReferenceOccurrence> Occurences) : Reference(Target)
    {
    }
}
