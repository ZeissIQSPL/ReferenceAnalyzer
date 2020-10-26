using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace ReferenceAnalyzer.UI.ViewModels
{
    public interface ISolutionViewModel 
    {
        string Path { get; }
        ReadOnlyObservableCollection<string> LastSolutions { get; }
        ReactiveCommand<Unit, Unit> PickSolutionFile { get; }
    }
}
