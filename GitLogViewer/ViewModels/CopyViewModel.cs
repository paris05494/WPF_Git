using GitLogViewer.Commands;
using GitLogViewer.Models;
using GitLogViewer.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GitLogViewer.ViewModels
{
    public class CopyViewModel : ViewModelBase
    {
        public ObservableCollection<PersonWrapper> People { get; } = new ObservableCollection<PersonWrapper>();
        public ObservableCollection<FolderTargetModel> Targets { get; } = new ObservableCollection<FolderTargetModel>();

        public ICommand SubmitCommand { get; }

        public CopyViewModel(List<PersonModel> people)
        {
            foreach (var p in people)
                People.Add(new PersonWrapper { IsSelected = false, Person = p });

            SubmitCommand = new RelayCommand(Submit);

            LoadTargetFolders();
        }

        private void LoadTargetFolders()
        {
            string basePath = @"C:\Users\paris\TortoiseGit\tortoisegit_tutorials\A\B\work\Anw";

            if (!Directory.Exists(basePath))
                return;

            var dirs = Directory.GetDirectories(basePath, "_Dummy*", SearchOption.TopDirectoryOnly);

            foreach (var dir in dirs)
            {
                string configPath = Path.Combine(dir, "ModelConfig.xml");
                if (File.Exists(configPath))
                {
                    Targets.Add(new FolderTargetModel
                    {
                        IsSelected = false,
                        FolderName = Path.GetFileName(dir),
                        FullPath = dir
                    });
                }
            }
        }

        private void Submit()
        {
            var selectedPeople = People
                .Where(p => p.IsSelected && p.Person != null)
                .Select(p => p.Person)
                .ToList();

            if (selectedPeople.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายชื่อเพื่อคัดลอก");
                return;
            }

            var selectedTargets = Targets
                .Where(t => t.IsSelected)
                .ToList();

            if (selectedTargets.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกโฟลเดอร์ปลายทาง");
                return;
            }

            foreach (var target in selectedTargets)
            {
                XmlParser.MergeModelConfig(target.FullPath, selectedPeople);
            }

            MessageBox.Show("คัดลอกและรวมข้อมูลสำเร็จ");

            // ปิดหน้าต่าง
            var window = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);

            if (window != null)
                window.Close();
        }
    }

    public class PersonWrapper
    {
        public bool IsSelected { get; set; }
        public PersonModel Person { get; set; }
    }

    public class FolderTargetModel
    {
        public bool IsSelected { get; set; }
        public string FolderName { get; set; }
        public string FullPath { get; set; } // ไม่แสดงใน UI แต่ใช้ merge
    }
}
