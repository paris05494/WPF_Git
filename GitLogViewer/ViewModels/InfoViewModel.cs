using GitLogViewer.Services;
using GitLogViewer.Commands;
using GitLogViewer.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GitLogViewer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GitLogViewer.ViewModels
{
    public class InfoViewModel : ViewModelBase
    {
        #region Fields
        private readonly GitService _service = new GitService();
        private string _gitRepo;
        private string _targetPath;
        private List<PersonModel> _copiedPeople = new List<PersonModel>();

        #endregion

        #region Constructors
        public InfoViewModel(string gitRepo)
        {
            _gitRepo = gitRepo;
            LoadFilesCommand = new RelayCommand(LoadFiles);
            DoubleClickCommand = new RelayCommand(OpenGitLogWindow);
            OpenCopyViewCommand = new RelayCommand(OpenCopyWindow);
            ReplaceCommand = new RelayCommand(Replace);
            LoadFiles();
        }

        #endregion

        #region ICommand
        public ICommand LoadFilesCommand { get; }
        public ICommand DoubleClickCommand { get; }
        public ICommand OpenCopyViewCommand { get; }
        public ICommand ReplaceCommand { get; }

        #endregion

        #region Properties
        public ObservableCollection<string> GitFiles { get; } = new ObservableCollection<string>();
        public ObservableCollection<PersonModel> People { get; } = new ObservableCollection<PersonModel>();

        #endregion

        #region Private methods
        private void LoadFiles()
        {
            GitFiles.Clear();
            if (string.IsNullOrEmpty(_gitRepo)) return;

            var files = _service.GetFiles(_gitRepo);
            foreach (var file in files)
                GitFiles.Add(file);
        }

        private void OpenGitLogWindow()
        {
            if (string.IsNullOrEmpty(_gitRepo)) return;

            var gitLogView = new GitLogView
            {
                DataContext = new GitLogViewModel(_gitRepo)
            };
            gitLogView.Show();
        }

        private void Replace()
        {
            if (string.IsNullOrEmpty(_targetPath) || _copiedPeople.Count == 0)
            {
                MessageBox.Show("ยังไม่มีข้อมูล Copy หรือไม่ได้เลือกปลายทาง");
                return;
            }

            XmlParser.MergeModelConfig(_targetPath, _copiedPeople);
            MessageBox.Show("เขียนข้อมูลใหม่เรียบร้อยแล้ว");

            LoadPeople(XmlParser.ParseModelConfig(_targetPath));
        }

        #endregion

        #region Public methods
        public void UpdatePath(string newPath)
        {
            _gitRepo = newPath;
            LoadFiles();
        }

        public void LoadPeople(List<PersonModel> people)
        {
            People.Clear();
            foreach (var p in people)
                People.Add(p);
        }
        private void OpenCopyWindow()
        {
            var copyView = new CopyView();
            var vm = new CopyViewModel(People.ToList());
            copyView.DataContext = vm;

            if (copyView.ShowDialog() == true)
            {
                _targetPath = vm.SelectedPath;
                _copiedPeople = vm.SelectedPeople.ToList();
            }
        }

        #endregion

    }
}
