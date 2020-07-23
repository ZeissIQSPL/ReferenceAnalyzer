using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using ReferenceAnalyzer.Core.ProjectEdit;

namespace ReferenceAnalyzer.Core
{
    public class ReferenceAnalyzer : IReferenceAnalyzer
    {
        private readonly IMessageSink _messageSink;
        private readonly IProjectAccess _projectAccess;
        private List<Project> _projects = new List<Project>();

        public ReferenceAnalyzer(IMessageSink messageSink, IProjectAccess projectAccess)
        {
            _messageSink = messageSink;
            _projectAccess = projectAccess;

            InitializeMsBuild();
        }


        public IDictionary<string, string> BuildProperties { get; set; } = new Dictionary<string, string>();
        public bool ThrowOnCompilationFailures { get; set; } = true;
        public IProgress<double> ProgressReporter { get; set; } = new Progress<double>();

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
            ProgressReporter.Report(-1);
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

            _projects = loadedSolution.Projects
                .OrderBy(p => p.Name)
                .ToList();

            ProgressReporter.Report(1);

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

            var outputPath = Path.GetFullPath(Path.Combine(project.OutputFilePath!, ".."));

            var assemblies = Directory.GetFiles(outputPath, "*.dll")
                .Select(f => MetadataReference.CreateFromFile(f));

            compilation = compilation!.AddReferences(assemblies);

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
                .Where(g => _projects
                    .Where(p => p != project)
                    .Select(p => p.AssemblyName)
                    .Contains(g.Key.Name))
                .Select(g => new ActualReference(g.Key.Name, g))
                .OrderBy(r => r.Target);

            var editor = new ReferencesEditor(_projectAccess, project.FilePath);

            var definedReferences = editor.GetReferencedProjects()
                .OrderBy(n => n);

            return new ReferencesReport(project.Name, definedReferences, actualReferences);
        }

        public async IAsyncEnumerable<ReferencesReport> AnalyzeAll()
        {
            await foreach (var report in Analyze(_projects.Select(p => p.Name)))
                yield return report;
        }

        public async IAsyncEnumerable<ReferencesReport> Analyze(IEnumerable<string> projects)
        {
            var analyzedProjects = 0;
            var totalProjects = projects.Count();
            ProgressReporter.Report(0);
            foreach (var project in projects)
            {
                var report = await Analyze(project);
                ProgressReporter.Report((double)++analyzedProjects / totalProjects);
                yield return report;
            }
        }
    }
}
