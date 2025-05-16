using GitLogViewer.Models;
using GitLogViewer.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using GitLogViewer.Commands;
using GitLogViewer.Views;

namespace GitLogViewer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly GitService _gitService;

        //pathที่เลือกได้จาก dropdown
        public ObservableCollection<string> RepoPaths { get; } = new ObservableCollection<string>
        {
            @"",
            @""
        };

        //path ที่เลือกปัจจุบันจาก dropdown
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
                }
            }
        }


        public ObservableCollection<string> GitFiles { get; } = new ObservableCollection<string>();

        //คำสั่งที่ผูกกับปุ่มโหลดไฟล์
        public ICommand LoadFilesCommand { get; }

        //คำสั่งที่เปิด Git log viewer
        public ICommand OpenGitLogCommand { get; }

        public MainViewModel()
        {
            _gitService = new GitService();
            SelectedPath = RepoPaths.FirstOrDefault();

            LoadFilesCommand = new RelayCommand(LoadFiles);
            OpenGitLogCommand = new RelayCommand(OpenGitLog);

        }

        //โหลดรายชื่อไฟล์ใน repo ปัจจุบัน
        private void LoadFiles()
        {
            if (string.IsNullOrEmpty(SelectedPath)) return;

            GitFiles.Clear();
            var files = _gitService.GetFiles(SelectedPath);
            foreach (var file in files)
                GitFiles.Add(file);
        }

        //เปิดหน้าต่าง Git log viewer พร้อมส่ง path ปัจจุบันไปให้ ViewModel
        private void OpenGitLog()
        {
            if (string.IsNullOrEmpty(SelectedPath)) return;

            var gitLogView = new GitLogView
            {
                DataContext = new GitLogViewModel(_gitService, SelectedPath)
            };
            gitLogView.Show();
        }
    }
}
