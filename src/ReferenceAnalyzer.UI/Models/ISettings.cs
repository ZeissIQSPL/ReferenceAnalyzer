using System.Collections.Generic;
using System.Collections.Immutable;

namespace ReferenceAnalyzer.UI.Models
{
    public interface ISettings
    {
        string SolutionPath { get; set; }

        IImmutableList<string> LastLoadedSolutions { get; set; }
    }
}
