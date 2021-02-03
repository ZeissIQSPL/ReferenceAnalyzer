using Microsoft.CodeAnalysis;

namespace ReferenceAnalyzer.Core.Models
{
    public record ReferenceOccurrence(ITypeSymbol UsedType, ReferenceLocation Location)
    {
    }
}
