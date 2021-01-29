using System;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ReferenceAnalyzer.UI.ViewModels;

namespace ReferenceAnalyzer.UI.Views
{

    public class ProjectView : ReactiveUserControl<ProjectViewModel>
    {
        public ProjectView()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposableRegistration =>
            {
                this.OneWayBind(ViewModel,
                       viewModel => viewModel.Name,
                       view => view.ProjectName.Text)
                   .DisposeWith(disposableRegistration);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.State,
                        view => view.ProjectName.Background,
                        s => s switch
                        {
                            ProcessingState.Finished => Brushes.Green,
                            ProcessingState.NotStarted => Brushes.Gray,
                            ProcessingState.InProgress => Brushes.LightYellow,
                            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
                        })
                    .DisposeWith(disposableRegistration);
            });

        }

        public TextBlock ProjectName => this.FindControl<TextBlock>(nameof(ProjectName));
    }
}
