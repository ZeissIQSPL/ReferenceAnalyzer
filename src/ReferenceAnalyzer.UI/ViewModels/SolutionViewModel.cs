using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReferenceAnalyzer.UI.Models;
using ReferenceAnalyzer.UI.Services;

namespace ReferenceAnalyzer.UI.ViewModels
{

    public class SolutionViewModel : ReactiveObject
    {
        private ReadOnlyObservableCollection<string> _lastSolutions;
        private string _path;

        public SolutionViewModel(ISettings settings, ISolutionFilepathPicker solutionFilepathPicker)
        {
            SetupSettings(settings);
            SetupPickSolutionFile(solutionFilepathPicker);
        }

        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }
        public ReactiveCommand<Unit, Unit> PickSolutionFile { get; private set; }

        public ReadOnlyObservableCollection<string> LastSolutions => _lastSolutions;
        private void SetupSettings(ISettings settings)
        {
            _path = settings.SolutionPath;

            this.WhenAnyValue(viewModel => viewModel.Path)
                .Subscribe(x => settings.SolutionPath = x);

            var solutions = new SourceList<string>();
            settings.LastLoadedSolutions.Subscribe(Observer.Create<string>(o => solutions.Add(o)));
            solutions.Connect().Bind(out _lastSolutions).Subscribe();

            this.WhenAnyValue(viewModel => viewModel.Path)
                .Subscribe(x => { settings.SolutionPath = x; solutions.Add(x); });

        }
        private void SetupPickSolutionFile(ISolutionFilepathPicker solutionFilepathPicker)
        {
            PickSolutionFile = ReactiveCommand.CreateFromTask(() => SelectFilepath(solutionFilepathPicker));
        }

        private async Task SelectFilepath(ISolutionFilepathPicker solutionFilepathPicker)
        {
            var result = await solutionFilepathPicker.SelectSolutionFilePath();
            Path = result;
        }
    }
}
