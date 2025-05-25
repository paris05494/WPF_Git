using GitLogViewer.Models;
using GitLogViewer.Services;
using System.Collections.ObjectModel;

namespace GitLogViewer.ViewModels
{
    public class GitLogViewModel : ViewModelBase
    {
        #region Fields
        private readonly GitService _service = new GitService();
        private string _gitRepo;

        #endregion

        #region Constructors
        public GitLogViewModel(string gitRepo)
        {
            _gitRepo = gitRepo;
            LoadLogs();
        }

        #endregion

        #region Properties
        public ObservableCollection<GitLogEntry> GitLogs { get; } = new ObservableCollection<GitLogEntry>();

        #endregion

        #region Private methods
        private void LoadLogs()
        {
            GitLogs.Clear();
            if (string.IsNullOrEmpty(_gitRepo)) return;

            var logs = _service.GetGitLogs(_gitRepo);
            foreach (var log in logs)
                GitLogs.Add(log);
        }

        #endregion

        #region Public methods
        public void UpdatePath(string newPath)
        {
            _gitRepo = newPath;
            LoadLogs();
        }

        #endregion




    }
}
