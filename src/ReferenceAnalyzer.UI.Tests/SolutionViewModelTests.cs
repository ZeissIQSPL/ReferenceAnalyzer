using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Binding;
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
        private readonly SolutionViewModel _sut;
        private readonly Mock<ISettings> _settingsMock;
        private readonly Mock<ISolutionFilepathPicker> _slnFilepathPickerMock;
        private readonly ObservableCollectionExtended<string> _settingsLoadedSolutions = new ObservableCollectionExtended<string>();

        public SolutionViewModelTests()
        {
            _settingsMock = new Mock<ISettings>();
            _settingsMock.Setup(x => x.LastLoadedSolutions)
                .Returns(_settingsLoadedSolutions);
            _slnFilepathPickerMock = new Mock<ISolutionFilepathPicker>();


            _sut = new SolutionViewModel(_settingsMock.Object, _slnFilepathPickerMock.Object);

        }

        [Fact]
        public void ChangingPathSavedInSettings()
        {
            const string newPath = "newPath";
            var previousCount = _settingsLoadedSolutions.Count;
            _sut.Path = newPath;

            _settingsLoadedSolutions.Should().HaveCount(previousCount+1);
        }

        [Fact]
        public void ClickingClearButtonClearsListOfLastLoadedSolutions()
        {
            const string newPath = "newPath";
            _sut.Path = newPath;

            _settingsLoadedSolutions.Should().HaveCount(x => x > 0);

            _sut.ClearSolutionList.Execute().Subscribe();

            _settingsLoadedSolutions.Should().HaveCount(0);
        }

        [Fact]
        public void SelectingPathFromListChangesCurrentPath()
        {
            const string pathFromList = "M:/y/path";
            _settingsLoadedSolutions.Add(pathFromList);

            _sut.SelectedPath = pathFromList;

            _sut.Path.Should().Be(pathFromList);
        }

        [Fact]
        public void PickingFileFromFilepickerChangesCurrentPath()
        {
            const string expectedPath = "pathFrom/Picker";
            _sut.Path = "another/path";
            _slnFilepathPickerMock.Setup(x => x.SelectSolutionFilePath()).Returns(Task.FromResult(expectedPath));

            _sut.PickSolutionFile.Execute().Subscribe();

            _sut.Path.Should().Be(expectedPath);
        }

        [Fact]
        public void LoadingSixthPathDoesntExpandListPastFive()
        {
            PopulateLastSolutions();
            _sut.LastSolutions.Count.Should().Be(5);
        }
        [Fact]
        public void LoadingSixthPathMakesFirstPathTheFirstLoaded()
        {
            PopulateLastSolutions();

            const string expected = "newPath";
            _sut.Path = expected;

            _sut.LastSolutions.First().Should().Be(expected);
        }

        [Fact]
        public void SettingsAreSavedAfterChangingPath()
        {
            _sut.Path = "newPath";
            _settingsMock.Verify(x => x.SaveSettings());
        }

        [Fact]
        public void SettingsAreNotSavedAfterChangingPathToTheSamePath()
        {
            _sut.Path = "newPath";
            _settingsMock.Reset();
            _sut.Path = "newPath";

            _settingsMock.Verify(x => x.SaveSettings(), Times.Never);
        }

        private void PopulateLastSolutions()
        {
            for (var i = 0; i < 5; i++)
            {
                _sut.Path = $"{i}";
            }
        }
    }
}
