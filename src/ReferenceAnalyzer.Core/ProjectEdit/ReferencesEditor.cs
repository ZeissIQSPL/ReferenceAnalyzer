using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
            return GetReferencedProjectsPaths(projectPath)
                .Select(path => GetOutputAssemblyName(path) ?? Path.GetFileNameWithoutExtension(path));
        }

        public IEnumerable<string> GetReferencedProjectsPaths(string projectPath)
        {
            var root = GetProjectContent(projectPath).Root;
            return root.Descendants(WithNamespace(root, "ProjectReference"))
                .Select(n =>
                {
                    var relativePath = n.Attribute("Include").Value;
                    return Path.Combine(Path.Combine(projectPath, ".."), relativePath);
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

        public string? GetOutputAssemblyName(string projectPath)
        {
            var root = GetProjectContent(projectPath).Root;
            return root.Descendants(WithNamespace(root, "ProjectName")).FirstOrDefault()?.Value ??
                   root.Descendants(WithNamespace(root, "AssemblyName")).FirstOrDefault()?.Value;
        }

        public void RemoveReferencedProjects(string projectPath, IEnumerable<string> projects)
        {
            var doc = GetProjectContent(projectPath);
            var root = doc.Root;
            root.Descendants(WithNamespace(root, "ProjectReference"))
                .Where(n =>
                {
                    var path = n.Attribute("Include").Value;
                    var projectName = Path.GetFileNameWithoutExtension(path);
                    return projects.Contains(projectName);
                })
                .Remove();

            doc.Save(projectPath);
        }

        private XName WithNamespace(XElement root, string name)
        {
            var namespaceName = root.GetDefaultNamespace().NamespaceName;
            return XName.Get(name, namespaceName);
        }
    }
}
