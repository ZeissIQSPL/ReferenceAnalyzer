using System.Collections.ObjectModel;
using System.Reactive;
using DynamicData.Binding;
using ReactiveUI;

namespace ReferenceAnalyzer.UI.ViewModels
{
    public interface ISolutionViewModel 
    {
        string Path { get; }
        string SelectedPath { get; set; }
        ReadOnlyObservableCollection<string> LastSolutions { get; }
        ReactiveCommand<Unit, Unit> PickSolutionFile { get; }
    }
}
