using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

namespace ReferenceAnalyzer.Core
{
	public class ReferenceAnalyzer
	{
        private readonly IMessageSink _messageSink;
        private List<Project> _projects;

        public ReferenceAnalyzer(IMessageSink messageSink)
        {
            _messageSink = messageSink;

            InitializeMsBuild();
        }

        private static void InitializeMsBuild()
        {
            if (MSBuildLocator.IsRegistered)
                return;
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.First();
            MSBuildLocator.RegisterInstance(instance);
        }

        public async Task Load(string solution)
		{
            /*var properties = new Dictionary<string, string>() {
				{"AlwaysCompileMarkupFilesInSeparateDomain", "false" },
				{"Configuration", "Debug"},
				{"Platform", "x64"}
			};*/

			using var workspace = MSBuildWorkspace.Create(/*properties*/);

			var loadedSolution = await workspace.OpenSolutionAsync(solution);

            foreach (var d in workspace.Diagnostics)
            {
                _messageSink.Write($"{d.Kind}: {d.Message}");
            }

            if (workspace.Diagnostics.Any(d => d.Kind == WorkspaceDiagnosticKind.Failure))
            {
                var errors = workspace.Diagnostics
                    .Where(d => d.Kind == WorkspaceDiagnosticKind.Failure)
                    .Select(d => d.Message + "\n");
                throw new Exception("Failed opening solution: \n" + string.Concat(errors));
            }

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

            var compilationResult = compilation.Emit(new MemoryStream());

            foreach (var d in compilationResult.Diagnostics)
            {
                _messageSink.Write($"{d.Severity}: {d.GetMessage()}");
            }

            if (compilationResult.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                var errors = compilationResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.GetMessage() + "\n");
                throw new Exception($"Failed compiling {project.Name}: \n" + string.Concat(errors));
            }

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
		}
	}

    public interface IMessageSink
    {
        void Write(string message);
    }
}