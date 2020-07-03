using System;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using ReferenceAnalyzer.Core;
using ReferenceAnalyzer.WPF.Utilities;

namespace ReferenceAnalyzer.WPF
{
    public class AppViewModel : ReactiveObject
    {
        private string _Path;

        public AppViewModel(ISettings settings, IProjectProvider projectProvider)
        {
            _Path = settings.SolutionPath;
            var canLoad = this.WhenAnyValue(x => x.Path, path => !string.IsNullOrEmpty(path));

            Load = ReactiveCommand.Create(() =>
            {
                return projectProvider.GetReferences(Path);
            }, canLoad);


            this.WhenAnyValue(viewModel => viewModel.Path).Subscribe(x => settings.SolutionPath = x);
        }

        public string Path
        {
            get => _Path;
            set => this.RaiseAndSetIfChanged(ref _Path, value);
        }

        public ReactiveCommand<Unit, ReferencesReport> Load { get; }


    }
}
