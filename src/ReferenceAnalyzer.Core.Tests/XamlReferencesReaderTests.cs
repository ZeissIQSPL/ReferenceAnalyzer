using System.IO;
using FluentAssertions;
using Moq;
using ReferenceAnalyzer.Core.ProjectEdit;
using ReferenceAnalyzer.Core.Util;
using Xunit;

namespace ReferenceAnalyzer.Core.Tests
{
    public class XamlReferencesReaderTests
    {
        private readonly XamlReferencesReader _sut;
        private readonly Mock<IProjectAccess> _projectAccessMock;

        public XamlReferencesReaderTests()
        {
            _projectAccessMock = new Mock<IProjectAccess>();
            _sut = new XamlReferencesReader(_projectAccessMock.Object);
        }

        [Fact]
        public void Test()
        {
            var file = Path.Combine(TestsHelper.GetTestFilesLocation(), "SampleXamlFile.xml");
            var content = File.ReadAllText(file);

            var path = "test";
            _projectAccessMock.Setup(m => m.Read(path))
                .Returns(content);

            var result = _sut.GetReferences("test");

            result.Should().BeEquivalentTo("ReferenceAnalyzer.UI", "ReferenceAnalyzer.Core", "System.Runtime");
        }
    }
}
