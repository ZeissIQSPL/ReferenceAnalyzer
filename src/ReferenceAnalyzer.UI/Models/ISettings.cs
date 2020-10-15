using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive;

namespace ReferenceAnalyzer.UI.Models
{
    public interface ISettings
    {
        string SolutionPath { get; set; }

        IImmutableList<string> LastLoadedSolutions { get; set; }

        IObservable<Unit> Xd { get; }
    }
}
