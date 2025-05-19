using GitLogViewer.Models;
using GitLogViewer.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using GitLogViewer.Commands;
using GitLogViewer.Views;

namespace GitLogViewer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly GitService _gitService;

        public ObservableCollection<string> RepoPaths { get; } = new ObservableCollection<string>
    {
        @"",
        @""
    };

        private string _selectedPath;
        public string SelectedPath
        {
            get => _selectedPath;
            set
            {
                if (_selectedPath != value)
                {
                    _selectedPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand OpenGitLogCommand { get; }

        public InfoViewModel InfoVM { get; }

        public MainViewModel()
        {
            _gitService = new GitService();
            SelectedPath = RepoPaths.FirstOrDefault();

            OpenGitLogCommand = new RelayCommand(OpenGitLog);
            InfoVM = new InfoViewModel(_gitService, this);
        }

        private void OpenGitLog()
        {
            if (string.IsNullOrEmpty(SelectedPath)) return;

            var gitLogView = new GitLogView
            {
                DataContext = new GitLogViewModel(_gitService, SelectedPath)
            };
            gitLogView.Show();
        }
    }

}
