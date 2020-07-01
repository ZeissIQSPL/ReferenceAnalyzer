using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace ReferenceAnalyzer.Core
{
	public class ReferenceAnalyzer
	{
		private List<Project> _projects;

        public ReferenceAnalyzer()
        {
            if (MSBuildLocator.IsRegistered) return;
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.First();
            MSBuildLocator.RegisterInstance(instance);
        }

		public async Task Load(string solution)
		{
            var properties = new Dictionary<string, string>() {
				{"AlwaysCompileMarkupFilesInSeparateDomain", "false" },
				{"Configuration", "Debug"},
				{"Platform", "x64"}
			};

			using var workspace = MSBuildWorkspace.Create(properties);

			var loadedSolution = await workspace.OpenSolutionAsync(solution);
			_projects = loadedSolution.Projects.ToList();
		}

		public async Task<ReferencesReport> Analyze(string target)
		{
			var project = _projects.First(p => p.Name == target);

			return await Analyze(project);

		}

		private async Task<ReferencesReport> Analyze(Project project)
		{
			var compilation = await project.GetCompilationAsync();
			var visitor = new RoslynVisitor(compilation);

			visitor.VisitNamespace(compilation.Assembly.GlobalNamespace);

            var actualReferences = visitor.Occurrences
                .GroupBy(o => o.UsedType.ContainingAssembly)
                .Select(g => new ActualReference(g.Key.Name, g))
                .OrderBy(r => r.Target);

			var definedReferences = compilation.ReferencedAssemblyNames
                .Select(reference => reference.Name)
                .OrderBy(n => n);

			return new ReferencesReport(definedReferences, actualReferences);

			/*proj.ActualReferences = visitor.UsedTypeSymbols
				.Where(t => t.ContainingAssembly != null)
				.Select(r => r.ContainingAssembly.Name)
				.Select(name => new Reference()
				{
					Project = name,
					UsagesInSource = visitor.UsedTypeSymbols
						.Where(e => e.ContainingAssembly != null)
						.Count(e => e.ContainingAssembly.Name == name)
				})
				.OrderBy(n => n.Project)
				.ToHashSet();


			proj.FormalReferences = compilation.ReferencedAssemblyNames
				.Select(reference => new Reference() { Project = reference.Name, UsagesInSource = 1 })
				.OrderBy(n => n.Project);

			proj;*/
		}
	}
}