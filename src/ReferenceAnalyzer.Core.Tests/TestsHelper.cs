using System;
using System.IO;
using System.Reflection;

namespace ReferenceAnalyzer.Core.Tests
{
    internal class TestsHelper
    {
        public static string GetTestFilesLocation()
        {
            var codeBase = Assembly.GetExecutingAssembly().Location;
            var path = Path.Combine(codeBase.Split(new[] {"src"}, StringSplitOptions.RemoveEmptyEntries)[0],
                "src",
                "ReferenceAnalyzer.Core.Tests",
                "TestFiles");

            return new Uri(path).AbsolutePath;
        }
    }
}
