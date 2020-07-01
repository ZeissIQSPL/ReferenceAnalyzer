using System.Collections.Generic;

namespace ReferenceAnalyzer.Core
{
	public class ActualReference
	{
		public ActualReference(string target, IEnumerable<ReferenceOccurence> occurences)
		{
			Target = target;
			Occurences = occurences;
		}

		public IEnumerable<ReferenceOccurence> Occurences { get; }
		public string Target { get; }
	}
}