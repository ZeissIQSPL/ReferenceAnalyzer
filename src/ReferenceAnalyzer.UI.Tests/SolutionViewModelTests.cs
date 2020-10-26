using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using FluentAssertions;
using Moq;
using ReferenceAnalyzer.UI.Models;
using ReferenceAnalyzer.UI.Services;
using ReferenceAnalyzer.UI.ViewModels;
using Xunit;

namespace ReferenceAnalyzer.UI.Tests
{
    public class SolutionViewModelTests
    {
        private SolutionViewModel _sut;
        private Mock<ISettings> _settingsMock;
        private Mock<ISolutionFilepathPicker> _slnFilepathPickerMock;
        private const string Path = "C:/Sample/Path";
        public SolutionViewModelTests()
        {
            _settingsMock = new Mock<ISettings>();
            _settingsMock.Setup(x => x.SolutionPath).Returns(Path);
            _settingsMock.Setup(x => x.LastLoadedSolutions).Returns(ImmutableList.Create<string>());
            _slnFilepathPickerMock = new Mock<ISolutionFilepathPicker>();

            _sut = new SolutionViewModel(_settingsMock.Object, _slnFilepathPickerMock.Object);

        }

        [Fact]
        public void DefaultSolutionPathTakenFromSettings()
        {
            const string expected = Path;
           
            _sut.Path.Should().Be(expected);
        }

        [Fact]
        public void ChangingPathSavedInSettings()
        {
            const string newPath = "newPath";
            _sut.Path = newPath;

            _settingsMock.VerifySet(x => x.SolutionPath = newPath);
        }

    }
}
