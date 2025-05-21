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

        private string _selectedPath;
        public string SelectedPath
        {
            get => _selectedPath;
            set
            {
                if (_selectedPath != value)
                {
                    _selectedPath = value;
                    OnPropertyChanged(nameof(SelectedPath));

                    InfoVM.UpdatePath(_selectedPath);
                    GitLogVM.UpdatePath(_selectedPath);
                }
            }
        }

        public InfoViewModel InfoVM { get; }
        public GitLogViewModel GitLogVM { get; }

        public MainViewModel()
        {
            _selectedPath = RepoPaths.FirstOrDefault();
            GitLogVM = new GitLogViewModel(_selectedPath);
            InfoVM = new InfoViewModel(_selectedPath);
        }
    }
}
