﻿using ReactiveUI;
using ReferenceAnalyzer.WPF.Utilities;
using System.Reactive.Disposables;

namespace ReferenceAnalyzer.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ReactiveWindow<AppViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new AppViewModel(new Settings());

            this.WhenActivated(disposableRegistration =>
            {
                this.Bind(ViewModel,
                    viewModel => viewModel.Path,
                    view => view.Path.Text
                    ).DisposeWith(disposableRegistration);
            });
        }
    }
}
