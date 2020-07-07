using Microsoft.CodeAnalysis;

namespace ReferenceAnalyzer.Core
{
    public class ReferenceOccurrence
    {
        public ReferenceOccurrence(ITypeSymbol usedType, ReferenceLocation location)
        {
            UsedType = usedType;
            Location = location;
        }

        public ITypeSymbol UsedType { get; }
        public ReferenceLocation Location { get; }
    }
}
