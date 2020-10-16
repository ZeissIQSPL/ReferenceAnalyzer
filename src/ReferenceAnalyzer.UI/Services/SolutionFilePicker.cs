using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Dialogs;

namespace ReferenceAnalyzer.UI.Services
{
    public class SolutionFilepathPicker : ISolutionFilepathPicker
    {
        public async Task<string> SelectSolutionFilePath()
        {
            var fileDialog = new OpenFileDialog() { AllowMultiple = false };

            fileDialog.Filters.Add(new FileDialogFilter() { Extensions = new List<string> { "sln" } });

            var mainWindow = (Avalonia.Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;

            var result = await fileDialog.ShowManagedAsync(mainWindow ?? throw new Exception("Wrong ISolutionFilepathPicker used for this app!"));

            if (result != null)
            {
                return result.First();
            }
            return "";
        }
    }
}
