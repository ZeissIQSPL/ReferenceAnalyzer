using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ReferenceAnalyzer.Core.ProjectEdit
{
    public class ReferencesEditor
    {
        private readonly IProjectAccess _projectAccess;
        private readonly Lazy<XDocument> _content;

        public ReferencesEditor(IProjectAccess projectAccess, string projectPath)
        {
            _projectAccess = projectAccess;
            _content = new Lazy<XDocument>(() =>
            {
                var text = _projectAccess.Read(projectPath);
                return XDocument.Parse(text);
            });
        }

        public IEnumerable<string> GetReferencedProjects()
        {
            var root = _content.Value.Root;
            return root.Descendants(WithNamespace("ProjectReference"))
                .Select(n =>
                {
                    var path = n.Attribute("Include").Value;
                    return Path.GetFileNameWithoutExtension(path);
                });
        }

        public IEnumerable<string> GetReferencedPackages()
        {

            var root = _content.Value.Root;
            return root.Descendants(WithNamespace("PackageReference"))
                .Select(n =>
                {
                    var path = n.Attribute("Include").Value;
                    return Path.GetFileNameWithoutExtension(path);
                });
        }

        private XName WithNamespace(string name)
        {
            var namespaceName = _content.Value.Root.GetDefaultNamespace().NamespaceName;
            return XName.Get(name, namespaceName);
        }
    }
}
