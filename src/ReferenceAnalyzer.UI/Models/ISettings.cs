using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive;
using DynamicData.Binding;

namespace ReferenceAnalyzer.UI.Models
{
    public interface ISettings
    { 
        ObservableCollectionExtended<string> LastLoadedSolutions { get; }
        void SaveSettings();
    }
}
