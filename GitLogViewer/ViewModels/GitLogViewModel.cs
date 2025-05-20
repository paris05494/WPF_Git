using GitLogViewer.Models;
using GitLogViewer.Services;
using System.Collections.ObjectModel;

namespace GitLogViewer.ViewModels
{
    public class GitLogViewModel : ViewModelBase
    {
        private readonly GitService _service;

        public ObservableCollection<GitLogEntry> GitLogs { get; } = new ObservableCollection<GitLogEntry>();

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
                    LoadLogs(); //โหลดใหม่เมื่อ path เปลี่ยน
                }
            }
        }

        public GitLogViewModel(GitService service)
        {
            _service = service;
        }

        private void LoadLogs()
        {
            GitLogs.Clear();
            if (string.IsNullOrEmpty(SelectedPath)) return;

            var logs = _service.GetGitLogs(SelectedPath);
            foreach (var log in logs)
                GitLogs.Add(log);
        }
    }
}
