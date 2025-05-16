using GitLogViewer.Models;
using GitLogViewer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitLogViewer.ViewModels
{
    public class GitLogViewModel : ViewModelBase
    {
        public ObservableCollection<GitLogEntry> GitLogs { get; }

        public GitLogViewModel(GitService service, string repoPath)
        {
            GitLogs = new ObservableCollection<GitLogEntry>(service.GetGitLogs(repoPath));
        }
    }
}
