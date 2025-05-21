using GitLogViewer.Models;
using GitLogViewer.Services;
using System.Collections.ObjectModel;

namespace GitLogViewer.ViewModels
{
    public class GitLogViewModel : ViewModelBase
    {
        private readonly GitService _service = new GitService();

        public ObservableCollection<GitLogEntry> GitLogs { get; } = new ObservableCollection<GitLogEntry>();
        private string _selectedPath;

        public GitLogViewModel(string selectedPath)
        {
            _selectedPath = selectedPath;
            LoadLogs();
        }

        public void UpdatePath(string newPath)
        {
            _selectedPath = newPath;
            LoadLogs();
        }

        private void LoadLogs()
        {
            GitLogs.Clear();
            if (string.IsNullOrEmpty(_selectedPath)) return;

            var logs = _service.GetGitLogs(_selectedPath);
            foreach (var log in logs)
                GitLogs.Add(log);
        }
    }
}
