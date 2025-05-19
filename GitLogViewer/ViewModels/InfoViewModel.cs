using GitLogViewer.Services;
using GitLogViewer.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GitLogViewer.ViewModels
{
    public class InfoViewModel : ViewModelBase
    {
        private readonly GitService _gitService;
        private readonly MainViewModel _mainVM;

        public ObservableCollection<string> GitFiles { get; } = new ObservableCollection<string>();

        public ICommand LoadFilesCommand { get; }
        public ICommand OpenGitLogCommand { get; }

        public InfoViewModel(GitService gitService, MainViewModel mainVM)
        {
            _gitService = gitService;
            _mainVM = mainVM;

            LoadFilesCommand = new RelayCommand(LoadFiles);
            OpenGitLogCommand = new RelayCommand(OpenGitLog);
        }

        private void LoadFiles()
        {
            GitFiles.Clear();

            if (string.IsNullOrEmpty(_mainVM.SelectedPath)) return;

            var files = _gitService.GetFiles(_mainVM.SelectedPath);
            foreach (var file in files)
                GitFiles.Add(file);
        }

        private void OpenGitLog()
        {
            _mainVM.OpenGitLogCommand.Execute(null);
        }
    }
}
