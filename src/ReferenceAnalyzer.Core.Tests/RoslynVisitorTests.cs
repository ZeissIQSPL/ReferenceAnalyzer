using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ReferenceAnalyzer.Core.Tests
{
    public class RoslynVisitorTests
    {
        private RoslynVisitor _sut;
        private CSharpCompilation _compilation;

        public static IEnumerable<object[]> TestFiles => Directory.GetFiles(GetTestFilesLocation()).Select(s => new [] {Path.GetFileName(s)});

        private void LoadFile(string name)
        {
            var path = Path.Combine(GetTestFilesLocation(), name);

            var text = File.ReadAllText(path);

            var tree = CSharpSyntaxTree.ParseText(text);
            _compilation = CSharpCompilation.Create("TestCompilation", new[] { tree });
            _sut =  new RoslynVisitor(_compilation);
        }

        private static string GetTestFilesLocation()
        {
            var path =  Path.Combine(Assembly.GetExecutingAssembly().CodeBase.Split("src")[0],
                "src",
                "ReferenceAnalyzer.Core.Tests",
                "TestFiles");

            return new Uri(path).AbsolutePath;
        }

        [Theory]
        [MemberData(nameof(TestFiles))]
        public void MethodParameter(string fileName)
        {
            LoadFile(fileName);

            _sut.VisitNamespace(_compilation.Assembly.GlobalNamespace);

            _sut.Occurrences.Should().ContainSingle(o => o.UsedType.Name == "Expected");
        }
    }
}
