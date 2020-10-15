using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualBasic;

namespace ReferenceAnalyzer.UI.Models
{
    public class Settings : ISettings
    {
        private const string SolutionPathKey = "SolutionPath";
        private const string LastSolutionsKey = "LastSolutions";
        private readonly System.Configuration.Configuration _settings;

        public Settings() => _settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public string SolutionPath
        {
            get => _settings.AppSettings.Settings[SolutionPathKey]?.Value;
            set
            {
                var settings = _settings.AppSettings.Settings;
                if (settings[SolutionPathKey] == null)
                {
                    settings.Add(SolutionPathKey, value);
                }
                else
                {
                    settings[SolutionPathKey].Value = value;
                    if (!LastLoadedSolutions.Contains(value))
                    {
                        LastLoadedSolutions = LastLoadedSolutions.Add(value);
                    }
                }
                _settings.Save(ConfigurationSaveMode.Modified);
            }
        }

        public IImmutableList<string> LastLoadedSolutions {
            get {
                var serializedJson = _settings.AppSettings.Settings[SolutionPathKey]?.Value;
                return serializedJson == null ? JsonSerializer.Deserialize<IImmutableList<string>>(serializedJson) : ImmutableList.Create<string>();
            }
            set  {
                var settings = _settings.AppSettings.Settings;
                if (settings[LastSolutionsKey] == null)
                    settings.Add(LastSolutionsKey, JsonSerializer.Serialize(value));
                else
                    settings[LastSolutionsKey].Value = JsonSerializer.Serialize(value);
                _settings.Save(ConfigurationSaveMode.Modified);
                }
            }
    }
}
