namespace ReferenceAnalyzer.Core
{
	public class RoslynProjectProvider
	{
		public RoslynProjectProvider()
        {
			var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.First();
			MSBuildLocator.RegisterInstance(instance);
		}

        public async IAsyncEnumerable<Project> GetProjectsFrom(string solutionPath, IProgress<int> progress)
		{
            progress.Report(-1);
			var properties = new Dictionary<string, string>() {
				{"AlwaysCompileMarkupFilesInSeparateDomain", "false" },
				{"Configuration", "Debug"},
				{"Platform", "x64"}
			};

            using var workspace = MSBuildWorkspace.Create(properties);

            var solution = await workspace.OpenSolutionAsync(solutionPath);
            var projects = solution.Projects.ToList();
            for (var i = 0; i < projects.Count; i++)
            {
                var project = projects[i];
                var compilation = await project.GetCompilationAsync();
                var proj = new Project { Name = project.Name, Path = project.FilePath };
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
					.Select(reference => new Reference() { Project = reference.Name, UsagesInSource = 1})
					.OrderBy(n => n.Project);

                progress.Report(100 * (i+1) / projects.Count);
                yield return proj;
            }
        }
	}
}