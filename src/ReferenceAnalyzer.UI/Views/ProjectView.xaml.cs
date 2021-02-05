using System;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ReferenceAnalyzer.Core.Models;

namespace ReferenceAnalyzer.UI.Views
{

    public class ProjectView : ReactiveUserControl<Project>
    {
        public ProjectView()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposableRegistration =>
            {
                this.OneWayBind(ViewModel,
                       viewModel => viewModel.Name,
                       view => view.DisplayName.Text)
                   .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.AnalysisStage,
                        view => view.Progress.Background,
                        s => s switch
                        {
                            EAnalysisStage.Finished => Brushes.Green,
                            EAnalysisStage.NotStarted => Brushes.Gray,
                            EAnalysisStage.InProgress => Brushes.LightYellow,
                            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
                        })
                    .DisposeWith(disposableRegistration);
            });

        }

        public TextBlock DisplayName => this.FindControl<TextBlock>(nameof(DisplayName));
        public TextBlock Progress => this.FindControl<TextBlock>(nameof(Progress));
    }
}
