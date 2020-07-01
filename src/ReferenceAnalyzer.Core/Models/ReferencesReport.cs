using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ReferenceAnalyzer.Core
{
	public class ReferencesReport
	{
		public ReferencesReport(IEnumerable<string> definedReferences, IEnumerable<ActualReference> actualReferences)
		{
			DefinedReferences = definedReferences;
			ActualReferences = actualReferences;
		}

		public IEnumerable<ActualReference> ActualReferences { get; }

		public IEnumerable<string> DefinedReferences { get; }

		public int ReferencesTo(string target)
		{
			var reference = ActualReferences.FirstOrDefault(r => r.Target == target);
			return reference == null ? 0 : reference.Occurrences.Count();
        }
	}
}