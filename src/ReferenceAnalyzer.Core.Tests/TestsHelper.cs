using System;
using System.IO;
using System.Reflection;

namespace ReferenceAnalyzer.Core.Tests
{
    internal class TestsHelper
    {
        public static string GetTestFilesLocation()
        {
            var path =  Path.Combine(Assembly.GetExecutingAssembly().CodeBase.Split(new [] {"src"}, StringSplitOptions.RemoveEmptyEntries)[0],
                "src",
                "ReferenceAnalyzer.Core.Tests",
                "TestFiles");

            return new Uri(path).AbsolutePath;
        }
    }
}
