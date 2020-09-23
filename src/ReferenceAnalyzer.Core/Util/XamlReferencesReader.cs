using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ReferenceAnalyzer.Core.ProjectEdit;

namespace ReferenceAnalyzer.Core.Util
{
    public class XamlReferencesReader : IXamlReferencesReader
    {
        private readonly IProjectAccess _projectAccess;

        public XamlReferencesReader(IProjectAccess projectAccess)
        {
            _projectAccess = projectAccess;
        }

        private XDocument GetFileContent(string projectPath)
        {
            var text = _projectAccess.Read(projectPath);
            return XDocument.Parse(text);
        }

        public IEnumerable<string> GetReferences(string path)
        {
            var doc = GetFileContent(path);

            var attributes = doc.Root.Attributes();

            return attributes
                .Where(a => a.Value.Contains("assembly="))
                .Select(a => a.Value.Split("assembly=").Last());
        }
    }
}
