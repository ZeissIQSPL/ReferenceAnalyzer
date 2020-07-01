
using System.Configuration;

namespace ReferenceExplorer.WPF {
    
    public class Settings : ISettings
    {
        private readonly Configuration _Settings;
        private const string _SolutionPathKey = "SolutionPath";
         public string SolutionPath {
            get => _Settings.AppSettings.Settings[_SolutionPathKey]?.Value;
            set  {
                var settings = _Settings.AppSettings.Settings;
                if (settings[_SolutionPathKey] == null)  
                {  
                    settings.Add(_SolutionPathKey, value);  
                }  
                else  
                {  
                    settings[_SolutionPathKey].Value = value;  
                }  
                _Settings.Save(ConfigurationSaveMode.Modified);  
             }
         }
        public Settings()
        {
             _Settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);  
        }
    }

}