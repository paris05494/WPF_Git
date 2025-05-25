using GitLogViewer.Services;
using GitLogViewer.Commands;
using GitLogViewer.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GitLogViewer.ViewModels
{
    public class InfoViewModel : ViewModelBase
    {
        #region Field
        private readonly GitService _service = new GitService();
        private string _gitRepo;

        #endregion

        #region Constructors
        public InfoViewModel(string gitRepo)
        {
            _gitRepo = gitRepo;
            LoadFilesCommand = new RelayCommand(LoadFiles);
            DoubleClickCommand = new RelayCommand(OpenGitLogWindow);
            LoadFiles();
        }

        #endregion

        #region ICommand
        public ICommand LoadFilesCommand { get; }
        public ICommand DoubleClickCommand { get; }

        #endregion

        #region Properties
        public ObservableCollection<string> GitFiles { get; } = new ObservableCollection<string>();

        #endregion

        #region Private methods
        private void LoadFiles()
        {
            GitFiles.Clear();
            if (string.IsNullOrEmpty(_gitRepo)) return;

            var files = _service.GetFiles(_gitRepo);
            foreach (var file in files)
                GitFiles.Add(file);
        }

        private void OpenGitLogWindow()
        {
            if (string.IsNullOrEmpty(_gitRepo)) return;

            var gitLogView = new GitLogView
            {
                DataContext = new GitLogViewModel(_gitRepo)
            };
            gitLogView.Show();
        }

        #endregion

        #region Public methods
        public void UpdatePath(string newPath)
        {
            _gitRepo = newPath;
            LoadFiles();
        }

        #endregion

    }
}
