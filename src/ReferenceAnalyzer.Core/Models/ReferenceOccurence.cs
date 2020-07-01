namespace ReferenceAnalyzer.Core
{
	public class ReferenceOccurence
	{
		public ReferenceOccurence(EReferenceType type, ReferenceLocation location)
		{
			Type = type;
			Location = location;
		}

		public EReferenceType Type { get; }
		public ReferenceLocation Location { get; }
	}
}