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
        private string _selectedFolderPath;
        private string _selectedFolderName;

        #endregion

        #region Constructors
        public MainViewModel(IFolderDialogService folderDialogService)
        {
            SelectFolderCommand = new RelayCommand(SelectFolder);
            _gitRepo = RepoPaths.FirstOrDefault();
            GitLogVM = new GitLogViewModel(_gitRepo);
            InfoVM = new InfoViewModel(_gitRepo);
        }

        #endregion

        #region ICommand
        public ICommand SelectFolderCommand { get; }

        #endregion

        #region Propertys

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

        public string SelectedFolderPath
        {
            get => _selectedFolderPath;
            set
            {
                if (_selectedFolderPath != value)
                {
                    _selectedFolderPath = value;
                    OnPropertyChanged(nameof(SelectedFolderPath));
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
                SelectedFolderPath = dialog.FileName;
                SelectedFolderName = Path.GetFileName(dialog.FileName);
            }
        }

        #endregion

        #region Public methods

        #endregion

    }
}
