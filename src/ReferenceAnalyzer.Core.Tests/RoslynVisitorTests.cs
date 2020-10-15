using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace ReferenceAnalyzer.Core.Tests
{
    public class RoslynVisitorTests
    {
        private ReferencesWalker _sut;
        private CSharpCompilation _compilation;

        public static IEnumerable<object[]> TestFiles =>
            Directory.GetFiles(TestsHelper.GetTestFilesLocation(), "*.cs")
                .Select(s => new[]
                {
                    Path.GetFileName(s)
                });

        private void LoadFile(string name)
        {
            var path = Path.Combine(TestsHelper.GetTestFilesLocation(), name);

            var text = File.ReadAllText(path);

            var tree = CSharpSyntaxTree.ParseText(text);
            _compilation = CSharpCompilation.Create("TestCompilation", new[] { tree });
            _sut =  new ReferencesWalker(_compilation, new Func<string, bool>[]{});
        }

        [Theory]
        [MemberData(nameof(TestFiles))]
        public void VisitorTest(string fileName)
        {
            LoadFile(fileName);

            foreach (var tree in _compilation.SyntaxTrees)
            {
                var testClass = tree
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .First(n => n.Identifier.Text == "Test");

                testClass.Accept(_sut);
            }

            _sut.Occurrences.Should().Contain(o => o.UsedType.Name == "Expected");
            _sut.Occurrences.Should().NotContain(o => o.UsedType.Name == "Unexpected");
        }
    }
}
