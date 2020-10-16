using System.Threading.Tasks;

namespace ReferenceAnalyzer.UI.Services
{
    public interface ISolutionFilepathPicker
    {
        Task<string> SelectSolutionFilePath();
    }
}
