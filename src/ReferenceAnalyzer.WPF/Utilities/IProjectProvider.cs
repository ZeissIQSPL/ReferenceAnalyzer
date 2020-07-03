using ReferenceAnalyzer.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReferenceAnalyzer.WPF.Utilities
{
    public interface IProjectProvider
    {
        ReferencesReport GetReferences(string path);
    }
}
