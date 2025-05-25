using GitLogViewer.Commands;
using GitLogViewer.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace GitLogViewer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private string _gitRepo;
        private string _workPath;
        private string _selectedFolderPath;
        private string _selectedFolderName;
        private readonly UserSettingService _userSettingService;


        #endregion

        #region Constructors
        public MainViewModel(IFolderDialogService folderDialogService)
        {
            SelectFolderCommand = new RelayCommand(SelectFolder);
            _userSettingService = new UserSettingService();

            // Load persisted work path
            WorkPath = _userSettingService.LoadWorkPath();

            // Set default GitRepo from list
            _gitRepo = RepoPaths.FirstOrDefault();

            // Init ViewModels
            GitLogVM = new GitLogViewModel(_gitRepo);
            InfoVM = new InfoViewModel(_gitRepo);
        }

        #endregion

        #region ICommand
        public ICommand SelectFolderCommand { get; }

        #endregion

        #region Properties

        // GitRepo
        public InfoViewModel InfoVM { get; }
        public GitLogViewModel GitLogVM { get; }
        public ObservableCollection<string> RepoPaths { get; } = new ObservableCollection<string>
        {
            @"C:\Users\paris\TortoiseGit\tortoisegit_tutorials",
            @"C:\Users\paris\TortoiseGit\tortoisegit_tests"
        };
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

        // Select Folder
        public string WorkPath
        {
            get => _workPath;
            set
            {
                if (_workPath != value)
                {
                    _workPath = value;
                    _userSettingService.SaveWorkPath(_workPath);
                    OnPropertyChanged(nameof(WorkPath));

                    SelectedFolderName = Path.GetFileName(_workPath);
                }
            }
        }

        public string SelectedFolderName
        {
            get => _selectedFolderName;
            set
            {
                if (_selectedFolderName != value)
                {
                    _selectedFolderName = value;
                    OnPropertyChanged(nameof(SelectedFolderName));
                }
            }
        }

        #endregion

        #region Private methods

        private void SelectFolder()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "เลือกโฟลเดอร์"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                WorkPath = dialog.FileName;
                SelectedFolderName = Path.GetFileName(dialog.FileName);
            }
        }

        #endregion

        #region Public methods

        #endregion

    }
}
