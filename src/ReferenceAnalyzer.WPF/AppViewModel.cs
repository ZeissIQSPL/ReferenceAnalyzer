using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReferenceAnalyzer.Core;
using ReferenceAnalyzer.WPF.Utilities;

namespace ReferenceAnalyzer.WPF
{
    public class AppViewModel : ReactiveObject
    {
        private string _Path;
        private ReferencesReport _SelectedProject;
        private readonly ReadOnlyObservableCollection<ReferencesReport> _Projects;


        public AppViewModel(ISettings settings, IReferenceAnalyzer projectProvider)
        {
            projectProvider.BuildProperties = new Dictionary<string, string>()
                {
                    {"AlwaysCompileMarkupFilesInSeparateDomain", "false" },
                    {"Configuration", "Debug"},
                    {"Platform", "x64"}
                };
            _Path = settings.SolutionPath;
            var canLoad = this.WhenAnyValue(x => x.Path, path => !string.IsNullOrEmpty(path));

            Load = ReactiveCommand.CreateFromObservable(() => 
                Observable.Create<ReferencesReport>(o => 
                    LoadReferencesReports(projectProvider, o)), canLoad);


            Load.ToObservableChangeSet()
                .Bind(out _Projects)
                .Subscribe();


            this.WhenAnyValue(viewModel => viewModel.Path).Subscribe(x => settings.SolutionPath = x);
        }

        private async Task LoadReferencesReports(IReferenceAnalyzer projectProvider, IObserver<ReferencesReport> o)
        {
            await foreach (var element in projectProvider.AnalyzeAll(Path))
            {
                o.OnNext(element);
            }
        }

        public string Path
        {
            get => _Path;
            set => this.RaiseAndSetIfChanged(ref _Path, value);
        }

        public ReferencesReport SelectedProject
        {
            get => _SelectedProject;
            set => this.RaiseAndSetIfChanged(ref _SelectedProject, value);
        }


        public ReadOnlyObservableCollection<ReferencesReport> Projects => _Projects;

        public ReactiveCommand<Unit, ReferencesReport> Load { get; }


    }
}
