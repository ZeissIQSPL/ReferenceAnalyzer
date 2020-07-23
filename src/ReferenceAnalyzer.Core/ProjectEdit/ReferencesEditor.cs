using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ReferenceAnalyzer.Core.ProjectEdit
{
    public class ReferencesEditor : IReferencesEditor
    {
        private readonly IProjectAccess _projectAccess;

        public ReferencesEditor(IProjectAccess projectAccess)
        {
            _projectAccess = projectAccess;
        }

        private XDocument GetProjectContent(string projectPath)
        {
            var text = _projectAccess.Read(projectPath);
            return XDocument.Parse(text);
        }

        public IEnumerable<string> GetReferencedProjects(string projectPath)
        {
            var root = GetProjectContent(projectPath).Root;
            return root.Descendants(WithNamespace(root, "ProjectReference"))
                .Select(n =>
                {
                    var path = n.Attribute("Include").Value;
                    return Path.GetFileNameWithoutExtension(path);
                });
        }

        public IEnumerable<string> GetReferencedPackages(string projectPath)
        {

            var root = GetProjectContent(projectPath).Root;
            return root.Descendants(WithNamespace(root, "PackageReference"))
                .Select(n =>
                {
                    var path = n.Attribute("Include").Value;
                    return Path.GetFileNameWithoutExtension(path);
                });
        }

        private XName WithNamespace(XElement root, string name)
        {
            var namespaceName = root.GetDefaultNamespace().NamespaceName;
            return XName.Get(name, namespaceName);
        }
    }
}
