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
        private readonly Mock<IProjectAccess> _accessMock;
        private readonly string _projectPath;
        private readonly string _noReferencesProjectPath;
        private readonly string _secondaryProjectPath;

        public ReferencesEditorTests()
        {
            var projectDir = @"C:\ProjectDir";
            _projectPath = $@"{projectDir}\Project.csproj";
            _secondaryProjectPath = $@"{projectDir}\Project2.csproj";
            _noReferencesProjectPath = @"C:\TestDir\TestProject.csproj";
            _accessMock = new Mock<IProjectAccess>();

            SetupFile("ProjectAndPackageReferences.csproj", _projectPath);
            SetupFile("Project2.csproj", _secondaryProjectPath);
            SetupFile("Project3.csproj", $@"{projectDir}\Project3.csproj");
            SetupFile("NoReferences.csproj", _noReferencesProjectPath);

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
            _sut.GetReferencedProjects(_projectPath).Should().BeEquivalentTo("Project2", "Project3Assembly");
        }

        [Fact]
        public void PackageReferencesRetrieved()
        {
            _sut.GetReferencedPackages(_projectPath).Should().BeEquivalentTo("CommandLineParser");
        }

        [Fact]
        public void FileContentIsCached()
        {
            _sut.GetReferencedProjects(_noReferencesProjectPath);
            _accessMock.Verify(m => m.Read(_noReferencesProjectPath), Times.Once);

            _sut.GetReferencedProjects(_noReferencesProjectPath);
            _accessMock.Verify(m => m.Read(_noReferencesProjectPath), Times.Once);
        }

        [Fact]
        public void CacheCanBeInvalidated()
        {
            _sut.GetReferencedProjects(_noReferencesProjectPath);
            _accessMock.Verify(m => m.Read(_noReferencesProjectPath), Times.Once);

            _sut.InvalidateCache();

            _sut.GetReferencedProjects(_noReferencesProjectPath);
            _accessMock.Verify(m => m.Read(_noReferencesProjectPath), Times.Exactly(2));
        }

        [Fact]
        public void SavingInvalidatesCache()
        {
            _sut.GetReferencedProjects(_projectPath);
            _sut.RemoveReferencedProjects(_projectPath, new []{"Project3Assembly"});

            _sut.GetReferencedProjects(_projectPath);

            _accessMock.Verify(m => m.Read(_projectPath), Times.Exactly(2));
        }

        [Fact]
        public void SavingInvalidatesOnlyCorrectProject()
        {

            _sut.GetReferencedProjects(_projectPath);
            _sut.GetReferencedProjects(_noReferencesProjectPath);
            _sut.RemoveReferencedProjects(_projectPath, new[] { "Project3Assembly" });

            _sut.GetReferencedProjects(_projectPath);
            _sut.GetReferencedProjects(_noReferencesProjectPath);

            _accessMock.Verify(m => m.Read(_projectPath), Times.Exactly(2));
            _accessMock.Verify(m => m.Read(_noReferencesProjectPath), Times.Once);
        }

        [Fact]
        public void CacheWorksForMultipleFiles()
        {
            _sut.GetReferencedProjects(_projectPath);
            _sut.GetReferencedProjects(_noReferencesProjectPath);

            _sut.GetReferencedProjects(_projectPath);
            _sut.GetReferencedProjects(_noReferencesProjectPath);

            _accessMock.Verify(m => m.Read(_projectPath), Times.Once);
            _accessMock.Verify(m => m.Read(_noReferencesProjectPath), Times.Once);
        }

        [Fact]
        public void CacheWorksForRelativePaths()
        {
            var testName = @"C:\TestDir\TestName.csproj";
            SetupFile("NoReferences.csproj", testName);

            _sut.GetReferencedProjects(testName);
            _sut.GetReferencedProjects(@"C:\TestDir\..\TestDir\TestName.csproj");
            _sut.GetReferencedProjects(@"C:\AnotherDir\..\TestDir\TestName.csproj");

            _accessMock.Verify(m => m.Read(testName));
            _accessMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void DirectAndIndirectAccessAreCached()
        {
            _sut.GetReferencedProjects(_projectPath);

            _accessMock.Verify(m => m.Read(_projectPath), Times.Once);
            _accessMock.Verify(m => m.Read(_secondaryProjectPath), Times.Once);

            _sut.GetReferencedProjects(_secondaryProjectPath);

            _accessMock.Verify(m => m.Read(_secondaryProjectPath), Times.Once);
        }
    }
}
