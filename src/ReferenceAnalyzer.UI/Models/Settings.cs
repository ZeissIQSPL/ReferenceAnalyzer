using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text.Json;
using Microsoft.VisualBasic;

namespace ReferenceAnalyzer.UI.Models
{
    public class Settings : ISettings
    {
        private const string SolutionPathKey = "SolutionPath";
        private const string LastSolutionsKey = "LastSolutions";
        private readonly Configuration _configuration;

        public Settings()
        {
            _configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public string SolutionPath
        {
            get => _configuration.AppSettings.Settings[SolutionPathKey]?.Value;
            set
            {
                var settings = _configuration.AppSettings.Settings;
                if (settings[SolutionPathKey]?.Value == null)
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
                _configuration.Save(ConfigurationSaveMode.Modified);
            }
        }



        public IImmutableList<string> LastLoadedSolutions
        {
            get
            {
                var serializedJson = _configuration.AppSettings.Settings[LastSolutionsKey]?.Value;
                return !string.IsNullOrEmpty(serializedJson) ?
                    JsonSerializer.Deserialize<IImmutableList<string>>(serializedJson) : ImmutableList.Create<string>();
            }
            set
            {
                var settings = _configuration.AppSettings.Settings;
                if (settings[LastSolutionsKey]?.Value == null)
                    settings.Add(LastSolutionsKey, JsonSerializer.Serialize(value));
                else
                {
                    settings[LastSolutionsKey].Value = JsonSerializer.Serialize(value);
                }
                    
                _configuration.Save(ConfigurationSaveMode.Modified);
            }
        }
    }
}
