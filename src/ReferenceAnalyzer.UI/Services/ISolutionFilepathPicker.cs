using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace ReferenceAnalyzer.UI.Services
{
    public interface ISolutionFilepathPicker
    {
        Task<string> SelectSolutionFilePath();
    }
}
