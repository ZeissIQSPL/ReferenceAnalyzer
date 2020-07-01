using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace ReferenceAnalyzer.Core
{
	public class ReferenceAnalyzer
	{
		private List<Project> _Projects;

		public async Task Load(string solution)
		{
			var properties = new Dictionary<string, string>() {
				{"AlwaysCompileMarkupFilesInSeparateDomain", "false" },
				{"Configuration", "Debug"},
				{"Platform", "x64"}
			};

			using var workspace = MSBuildWorkspace.Create(properties);

			var loadedSolution = await workspace.OpenSolutionAsync(solution);
			_Projects = loadedSolution.Projects.ToList();
		}

		public async Task<ReferencesReport> Analyze(string target)
		{
			var project = _Projects.First(p => p.Name == target);

			return await Analyze(project);

		}

		private async Task<ReferencesReport> Analyze(Project project)
		{
			var compilation = await project.GetCompilationAsync();
			var visitor = new RoslynVisitor(compilation);

			visitor.VisitNamespace(compilation.Assembly.GlobalNamespace);

			proj.ActualReferences = visitor.UsedTypeSymbols
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

			proj;

			var actualReferences = new[]
			{
				new ActualReference("", new []{new ReferenceOccurence(0, new ReferenceLocation())}),
				new ActualReference("", new []{new ReferenceOccurence(0, new ReferenceLocation())})

			};
			return new ReferencesReport(Enumerable.Empty<string>(), actualReferences);
		}
	}
}