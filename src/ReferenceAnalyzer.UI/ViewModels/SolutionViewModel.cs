using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text;
using ReactiveUI;

namespace ReferenceAnalyzer.UI.ViewModels
{

    public class SolutionViewModel : ReactiveObject
    {
        private string _path = "ehh";

        public SolutionViewModel()
        {
            Path = "something";
            ExampleCommand = ReactiveCommand.Create(() => { Path += "asdasdqwe112"; });
        }

        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        public ReactiveCommand<Unit, Unit> ExampleCommand { get; private set; }

    }
}
