using System.Collections.Generic;

namespace ReferenceAnalyzer.Core.Util
{
    public interface IXamlReferencesReader
    {
        IEnumerable<string> GetReferences(string path);
    }
}
