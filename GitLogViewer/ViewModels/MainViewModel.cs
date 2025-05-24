using GitLogViewer.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace GitLogViewer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<string> RepoPaths { get; } = new ObservableCollection<string>
        {
            @"C:\Users\paris\TortoiseGit\tortoisegit_tutorials",
            @"C:\Users\paris\TortoiseGit\tortoisegit_tests"
        };

        private string _gitRepo;
        public string GitRepo
        {
            get => _gitRepo;
            set
            {
                if (_gitRepo != value)
                {
                    _gitRepo = value;
                    OnPropertyChanged(nameof(GitRepo));

                    InfoVM.UpdatePath(_gitRepo);
                    GitLogVM.UpdatePath(_gitRepo);
                }
            }
        }

        public InfoViewModel InfoVM { get; }
        public GitLogViewModel GitLogVM { get; }

        public MainViewModel()
        {
            _gitRepo = RepoPaths.FirstOrDefault();
            GitLogVM = new GitLogViewModel(_gitRepo);
            InfoVM = new InfoViewModel(_gitRepo);
        }
    }
}
