using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text.Json;
using DynamicData;
using DynamicData.Binding;
using Microsoft.VisualBasic;

namespace ReferenceAnalyzer.UI.Models
{
    public class Settings : ISettings
    {
        private const string LastSolutionsKey = "LastSolutions";
        private readonly Configuration _configuration;

        public Settings()
        {
            _configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            LastLoadedSolutions = LoadSettings();
        }

        public ObservableCollectionExtended<string> LastLoadedSolutions { get; }

        private ObservableCollectionExtended<string> LoadSettings()
        {
            var serializedJson = _configuration.AppSettings.Settings[LastSolutionsKey]?.Value;
            if (string.IsNullOrEmpty(serializedJson))
            {
                return new ObservableCollectionExtended<string>();
            }
            var list = JsonSerializer.Deserialize<ObservableCollectionExtended<string>>(serializedJson);
            var observableCollection = new ObservableCollectionExtended<string>();
            observableCollection.AddRange(list);
            return observableCollection;
        }

        public void SaveSettings()
        {
            var settings = _configuration.AppSettings.Settings;
            if (settings[LastSolutionsKey]?.Value == null)
                settings.Add(LastSolutionsKey, JsonSerializer.Serialize(LastLoadedSolutions));
            else
            {
                settings[LastSolutionsKey].Value = JsonSerializer.Serialize(LastLoadedSolutions);
            }

            _configuration.Save(ConfigurationSaveMode.Modified);
        }
    }
}
