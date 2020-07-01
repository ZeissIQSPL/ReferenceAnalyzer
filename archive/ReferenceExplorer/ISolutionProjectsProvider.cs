using System;
using System.Collections.Generic;
using ReferenceExplorer.Models;

namespace ReferenceExplorer
{
    public interface ISolutionProjectsProvider
    {
        IAsyncEnumerable<Project> GetProjectsFrom(string slnPath, IProgress<int> progress);
    }
}