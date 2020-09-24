using System.Configuration;

namespace ReferenceAnalyzer.UI.Models
{
    public class Settings : ISettings
    {
        private const string SolutionPathKey = "SolutionPath";
        private readonly Configuration _settings;

        public Settings() => _settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public string SolutionPath
        {
            get => _settings.AppSettings.Settings[SolutionPathKey]?.Value;
            set
            {
                var settings = _settings.AppSettings.Settings;
                if (settings[SolutionPathKey] == null)
                    settings.Add(SolutionPathKey, value);
                else
                    settings[SolutionPathKey].Value = value;
                _settings.Save(ConfigurationSaveMode.Modified);
            }
        }
    }
}
