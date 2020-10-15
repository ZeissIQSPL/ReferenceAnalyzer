using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using ReferenceAnalyzer.Core.ProjectEdit;
using ReferenceAnalyzer.Core.Util;

namespace ReferenceAnalyzer.Core
{
    public class ReferenceAnalyzer : IReferenceAnalyzer
    {
        private readonly IMessageSink _messageSink;
        private readonly IReferencesEditor _editor;
        private readonly IXamlReferencesReader _xamlReferencesReader;
        private List<Project> _projects = new List<Project>();

        public ReferenceAnalyzer(IMessageSink messageSink, IReferencesEditor editor,
            IXamlReferencesReader xamlReferencesReader)
        {
            _messageSink = messageSink;
            _editor = editor;
            _xamlReferencesReader = xamlReferencesReader;

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
            if (project.FilePath == null)
                throw new InvalidOperationException("Attempted to analyze project without a disk path");

            var outputPath = Path.GetFullPath(Path.Combine(project.OutputFilePath!, ".."));

            Compilation compilation = await GetCompilation(project, outputPath);

            var ignoreRules = new Func<string, bool>[]
            {
                name => name == project.AssemblyName,
                name => name == "mscorlib",
                name => name.StartsWith("System")
            };

            var visitor = new ReferencesWalker(compilation, ignoreRules);

            var treeAnalysis = new List<Task>(compilation.SyntaxTrees.Count());
            var analysisTasks = compilation.SyntaxTrees
                .Select(tree => Task.Run(async () => visitor.Visit(await tree.GetRootAsync())));
            treeAnalysis.AddRange(analysisTasks);

            await Task.WhenAll(treeAnalysis);

            IEnumerable<string> xamlReferences = GetXamlReferences(project);
            xamlReferences = xamlReferences
                .Where(reference => ignoreRules.All(rule => !rule(reference)));

            var groupedAssemblies = visitor.Occurrences
                .GroupBy(o => o.UsedType.ContainingAssembly)
                .Where(g => g.Key != null);

            var actualReferences = groupedAssemblies
                .Select(g => new ActualReference(g.Key.Name, g))
                .Concat(xamlReferences
                    .Select(r => new ActualReference(r, Enumerable.Empty<ReferenceOccurrence>())))
                .Distinct()
                .OrderBy(r => r.Target);


            var definedReferences = _editor.GetReferencedProjects(project.FilePath)
                .OrderBy(n => n);

            return new ReferencesReport(project.Name, definedReferences, actualReferences, project.FilePath);
        }

        private IEnumerable<string> GetXamlReferences(Project project)
        {
            var xamlFiles = Directory.GetParent(project.FilePath)
                .GetFiles("*.xaml", SearchOption.AllDirectories)
                .Select(f => Path.Combine(f.Directory!.FullName, f.Name));

            var xamlReferences = xamlFiles
                .SelectMany(f => _xamlReferencesReader.GetReferences(f))
                .Distinct();
            return xamlReferences;
        }

        private async Task<Compilation> GetCompilation(Project project, string outputPath)
        {
            var compilation = await project.GetCompilationAsync();
            var referencedProjects = _editor.GetReferencedProjects(project.FilePath!);
            var assemblies = referencedProjects
                .Select(p =>
                {
                    var dll = Path.Combine(outputPath, p + ".dll");
                    if (File.Exists(dll))
                        return dll;
                    var exe = Path.Combine(outputPath, p + ".exe");

                    return File.Exists(exe) ? exe : string.Empty;
                })
                .Where(f => !string.IsNullOrEmpty(f))
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

            return compilation;
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
