using DynamicData.Binding;

namespace ReferenceAnalyzer.UI.Models
{
    public interface ISettings
    { 
        ObservableCollectionExtended<string> LastLoadedSolutions { get; }
        void SaveSettings();
    }
}
