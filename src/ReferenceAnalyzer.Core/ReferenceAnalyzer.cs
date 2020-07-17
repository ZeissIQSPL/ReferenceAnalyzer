using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace ReferenceAnalyzer.Core
{
    public class ReferenceAnalyzer : IReferenceAnalyzer
    {
        private readonly IMessageSink _messageSink;
        private List<Project> _projects = new List<Project>();

        public ReferenceAnalyzer(IMessageSink messageSink)
        {
            _messageSink = messageSink;

            InitializeMsBuild();
        }


        public IDictionary<string, string> BuildProperties { get; set; } = new Dictionary<string, string>();
        public bool ThrowOnCompilationFailures { get; set; } = true;

        public async IAsyncEnumerable<ReferencesReport> AnalyzeAll(string solutionPath)
        {
            await Load(solutionPath);
            await foreach (var a in AnalyzeAll())
                yield return a;
        }

        private static void InitializeMsBuild()
        {
            if (MSBuildLocator.IsRegistered)
                return;
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.First();
            MSBuildLocator.RegisterInstance(instance);
        }

        public async Task<IEnumerable<string>> Load(string solution)
        {
            using var workspace = MSBuildWorkspace.Create(BuildProperties);
            workspace.SkipUnrecognizedProjects = true;

            var loadedSolution = await workspace.OpenSolutionAsync(solution);

            foreach (var d in workspace.Diagnostics)
                _messageSink.Write($"{d.Kind}: {d.Message}");

            if (ThrowOnCompilationFailures &&
                workspace.Diagnostics.Any(d => d.Kind == WorkspaceDiagnosticKind.Failure))
            {
                var errors = workspace.Diagnostics
                    .Where(d => d.Kind == WorkspaceDiagnosticKind.Failure)
                    .Select(d => d.Message + "\n");
                throw new Exception("Failed opening solution: \n" + string.Concat(errors));
            }

            _projects = loadedSolution.Projects.ToList();

            return _projects.Select(p => p.Name);
        }

        public async Task<ReferencesReport> Analyze(string target)
        {
            var project = _projects.First(p => p.Name == target);

            return await Analyze(project);
        }

        private async Task<ReferencesReport> Analyze(Project project)
        {
            var compilation = await project.GetCompilationAsync();
            if (compilation == null)
                throw new ArgumentNullException(nameof(compilation));

            await using var dummy = new MemoryStream();
            var compilationResult = compilation.Emit(dummy);

            foreach (var d in compilationResult.Diagnostics)
                _messageSink.Write($"{d.Severity}: {d.GetMessage()}");

            if (ThrowOnCompilationFailures &&
                compilationResult.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                var errors = compilationResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.GetMessage() + "\n");
                throw new Exception($"Failed compiling {project.Name}: \n" + string.Concat(errors));
            }

            var visitor = new ReferencesWalker(compilation);

            foreach (var tree in compilation.SyntaxTrees)
                visitor.Visit(await tree.GetRootAsync());

            var actualReferences = visitor.Occurrences
                .GroupBy(o => o.UsedType.ContainingAssembly)
                .Where(g => g.Key != null)
                .Select(g => new ActualReference(g.Key.Name, g))
                .OrderBy(r => r.Target);

            var definedReferences = compilation.ReferencedAssemblyNames
                .Select(reference => reference.Name)
                .OrderBy(n => n);

            return new ReferencesReport(project.Name, definedReferences, actualReferences);
        }

        public async IAsyncEnumerable<ReferencesReport> AnalyzeAll()
        {
            foreach (var project in _projects)
                yield return await Analyze(project);
        }

        public async IAsyncEnumerable<ReferencesReport> Analyze(IEnumerable<string> projects)
        {
            foreach (var project in _projects.Where(p => projects.Contains(p.Name)))
                yield return await Analyze(project);
        }
    }
}
