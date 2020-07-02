using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReferenceAnalyzer.WPF.Utilities;

namespace ReferenceAnalyzer.WPF
{
    public class AppViewModel : ReactiveObject
    {
        private string _Path;

        public AppViewModel(ISettings settings)
        {
            _Path = settings.SolutionPath;
            var canLoad = this.WhenAnyValue(x => x.Path, path => !string.IsNullOrEmpty(path));

            this.WhenAnyValue(viewModel => viewModel.Path).Subscribe(x => settings.SolutionPath = x);
        }

        public string Path
        {
            get => _Path;
            set => this.RaiseAndSetIfChanged(ref _Path, value);
        }

    }
}
