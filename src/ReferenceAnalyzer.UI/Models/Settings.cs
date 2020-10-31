using System.Configuration;
using System.Text.Json;
using DynamicData.Binding;

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
            return JsonSerializer.Deserialize<ObservableCollectionExtended<string>>(serializedJson);
        }

        public void SaveSettings()
        {
            var settings = _configuration.AppSettings.Settings;
            if (settings[LastSolutionsKey]?.Value == null)
                settings.Add(LastSolutionsKey, JsonSerializer.Serialize(LastLoadedSolutions));
            else
                settings[LastSolutionsKey].Value = JsonSerializer.Serialize(LastLoadedSolutions);

            _configuration.Save(ConfigurationSaveMode.Modified);
        }
    }
}
