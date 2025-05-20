using GitLogViewer.Services;
using GitLogViewer.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GitLogViewer.Views;

namespace GitLogViewer.ViewModels
{
    public class InfoViewModel : ViewModelBase
    {
        private readonly GitService _gitService;

        public ObservableCollection<string> GitFiles { get; } = new ObservableCollection<string>();

        public ICommand LoadFilesCommand { get; }
        public ICommand DoubleClickCommand { get; }

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

        public GitService GitService { get; set; }
        public InfoViewModel(GitService gitService)
        {
            _gitService = new GitService();
            LoadFilesCommand = new RelayCommand(LoadFiles);
            DoubleClickCommand = new RelayCommand(DoubleClick);
        }

        private void LoadFiles()
        {
            GitFiles.Clear();

            if (string.IsNullOrEmpty(SelectedPath)) return;

            var files = _gitService.GetFiles(SelectedPath);
            foreach (var file in files)
                GitFiles.Add(file);
        }

        private void DoubleClick()
        {
            if (string.IsNullOrEmpty(SelectedPath)) return;

            var gitLogView = new GitLogView
            {
                DataContext = new GitLogViewModel(_gitService) { SelectedPath = this.SelectedPath }
            };
            gitLogView.Show();
        }
    }
}
