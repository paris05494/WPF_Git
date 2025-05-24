using GitLogViewer.Models;
using GitLogViewer.Services;
using System.Collections.ObjectModel;

namespace GitLogViewer.ViewModels
{
    public class GitLogViewModel : ViewModelBase
    {
        private readonly GitService _service = new GitService();

        public ObservableCollection<GitLogEntry> GitLogs { get; } = new ObservableCollection<GitLogEntry>();
        private string _gitRepo;

        public GitLogViewModel(string gitRepo)
        {
            _gitRepo = gitRepo;
            LoadLogs();
        }

        public void UpdatePath(string newPath)
        {
            _gitRepo = newPath;
            LoadLogs();
        }

        private void LoadLogs()
        {
            GitLogs.Clear();
            if (string.IsNullOrEmpty(_gitRepo)) return;

            var logs = _service.GetGitLogs(_gitRepo);
            foreach (var log in logs)
                GitLogs.Add(log);
        }
    }
}
