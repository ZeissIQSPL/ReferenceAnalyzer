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

            SetupFile("ProjectAndPackageReferences.csproj", _projectName);
            SetupFile("Project2.csproj", $@"{_projectName}\..\.\Project2.csproj");
            SetupFile("Project3.csproj", $@"{_projectName}\..\.\Project3.csproj");

            _sut = new ReferencesEditor(_accessMock.Object);
        }

        private void SetupFile(string fileName, string projectPath)
        {
            var projectFile = Path.Combine(TestsHelper.GetTestFilesLocation(), fileName);
            var content = File.ReadAllText(projectFile);
            _accessMock.Setup(m => m.Read(projectPath))
                .Returns(content);
        }

        [Fact]
        public void ProjectReferencesRetrieved()
        {
            _sut.GetReferencedProjects(_projectName).Should().BeEquivalentTo("Project2", "Project3Assembly");
        }

        [Fact]
        public void PackageReferencesRetrieved()
        {
            _sut.GetReferencedPackages(_projectName).Should().BeEquivalentTo("CommandLineParser");
        }
    }
}
