using System.IO;
using FluentAssertions;
using Moq;
using ReferenceAnalyzer.Core.ProjectEdit;
using Xunit;

namespace ReferenceAnalyzer.Core.Tests
{
    public class ReferencesEditorTests
    {
        private readonly ReferencesEditor _sut;
        private readonly string _projectName;
        private readonly Mock<IProjectAccess> _accessMock;

        public ReferencesEditorTests()
        {
            _projectName = "project";
            _accessMock = new Mock<IProjectAccess>();

            var projectFile = Path.Combine(TestsHelper.GetTestFilesLocation(), "ProjectAndPackageReferences.xml");
            var content = File.ReadAllText(projectFile);
            _accessMock.Setup(m => m.Read(_projectName))
                .Returns(content);

            _sut = new ReferencesEditor(_accessMock.Object, _projectName);
        }

        [Fact]
        public void ProjectContentIsReadOnlyOnce()
        {
            _sut.GetReferencedProjects();

            _accessMock.Verify(m => m.Read(_projectName), Times.Once);

            _sut.GetReferencedProjects();

            _accessMock.Verify(m => m.Read(_projectName), Times.Once);
        }

        [Fact]
        public void ProjectReferencesRetrieved()
        {
            _sut.GetReferencedProjects().Should().BeEquivalentTo("Project2");
        }

        [Fact]
        public void PackageReferencesRetrieved()
        {
            _sut.GetReferencedPackages().Should().BeEquivalentTo("CommandLineParser");
        }
    }
}
