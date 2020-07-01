using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReferenceExplorer.Models;

namespace ReferenceExplorer.WPF
{
    public class AppViewModel : ReactiveObject
    {
        private string _Path;
        private readonly ReadOnlyObservableCollection<Project> _Projects;
        private readonly ObservableAsPropertyHelper<int> _Count;
        private float _Progress;
        private Project _SelectedProject;

        public AppViewModel(ISolutionProjectsProvider solutionProvider, ISettings settings)
        {
            _Path = settings.SolutionPath;
            var canLoad = this.WhenAnyValue(x => x.Path, path => !string.IsNullOrEmpty(path));
            Load = ReactiveCommand.CreateFromObservable(
                 () => Observable.Create<Project>(o => LoadProjects(o, solutionProvider)),
                 canLoad);

            Load.ThrownExceptions.Subscribe(ex => MessageBox.Show(ex.Message));

            Load.ToObservableChangeSet()
                .Bind(out _Projects)
                .Subscribe();

            _Count = _Projects
                .ToObservableChangeSet()
                .Select(_ => Projects.Count)
                .ToProperty(this, x => x.Count);

            this.WhenAnyValue(viewModel => viewModel.Path).Subscribe(x => settings.SolutionPath = x);
        }



        public ReadOnlyObservableCollection<Project> Projects => _Projects;

        public string Path
        {
            get => _Path;
            set => this.RaiseAndSetIfChanged(ref _Path, value);
        }

        public ReactiveCommand<Unit, Project> Load { get; }

        public Project SelectedProject
        {
            get => _SelectedProject;
            set => this.RaiseAndSetIfChanged(ref _SelectedProject, value);
        }

        public float Progress { get => _Progress; set => this.RaiseAndSetIfChanged(ref _Progress, value); }

        public int Count => _Count.Value;


        private async Task<IDisposable> LoadProjects(IObserver<Project> observer, ISolutionProjectsProvider solutionProvider)
        {
            var progress = new Progress<int>(i => Progress = i);
            try
            {
                var asyncEnumerable = solutionProvider.GetProjectsFrom(Path, progress);
                await foreach (var project in asyncEnumerable)
                {
                    observer.OnNext(project);
                }
            }
            catch (Exception e)
            {
	            observer.OnError(e);
            }
            finally
            {
                observer.OnCompleted();
            }
            return Disposable.Empty;
        }
    }
}