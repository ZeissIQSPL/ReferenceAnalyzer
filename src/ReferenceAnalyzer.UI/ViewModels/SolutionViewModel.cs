

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReferenceAnalyzer.UI.Models;
using ReferenceAnalyzer.UI.Services;

namespace ReferenceAnalyzer.UI.ViewModels
{

    public class SolutionViewModel : ReactiveObject, ISolutionViewModel
    {
        private const int LAST_SOLUTION_LIST_LIMIT = 5;
        private string _path;
        private string _selectedPath;
        private ReadOnlyObservableCollection<string> _lastSolutions;

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

        public string SelectedPath
        {
            get => _selectedPath;
            set => this.RaiseAndSetIfChanged(ref _selectedPath, value);
        }
        private void SetupSettings(ISettings settings)
        {
            settings.LastLoadedSolutions.ToObservableChangeSet().Filter(x => x != null).Bind(out _lastSolutions).Subscribe();

            this.WhenAnyValue(viewModel => viewModel.Path)
                .Subscribe(x => {
                    var count = settings.LastLoadedSolutions.Count;
                        if (!settings.LastLoadedSolutions.Contains(x))
                        {
                            if (count > LAST_SOLUTION_LIST_LIMIT)
                            {
                                settings.LastLoadedSolutions.RemoveAt(0);
                            }
                            settings.LastLoadedSolutions.Add(x);
                            settings.SaveSettings();
                        }
                });

            this.WhenAnyValue(viewModel => viewModel.SelectedPath).Subscribe(x => Path = x);

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
