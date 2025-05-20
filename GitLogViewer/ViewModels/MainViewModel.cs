using GitLogViewer.Services;
using System.Collections.ObjectModel;
using System.Linq;
using GitLogViewer.ViewModels;

namespace GitLogViewer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly GitService _gitService;

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
                    OnPropertyChanged();

                    if (InfoVM != null)
                        InfoVM.SelectedPath = value; // sync ไปที่ InfoVM
                }
            }
        }

        public InfoViewModel InfoVM { get; }
        public GitLogViewModel gitLogVM { get; }

        public MainViewModel()
        {
            gitLogVM = new GitLogViewModel(_gitService);
            InfoVM = new InfoViewModel(_gitService);
            _gitService = new GitService();
            SelectedPath = RepoPaths.FirstOrDefault();
        }
    }
}
