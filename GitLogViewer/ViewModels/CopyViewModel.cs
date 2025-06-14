using GitLogViewer.Commands;
using GitLogViewer.Models;
using GitLogViewer.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GitLogViewer.ViewModels
{
    public class CopyViewModel : ViewModelBase
    {
        private string _targetPath;

        public ObservableCollection<PersonWrapper> People { get; } = new ObservableCollection<PersonWrapper>();
        public ICommand SubmitCommand { get; }
        public string SelectedPath { get; private set; } = "";
        public List<PersonModel> SelectedPeople { get; private set; } = new List<PersonModel>();

        public CopyViewModel(List<PersonModel> people)
        {
            foreach (var p in people)
                People.Add(new PersonWrapper { IsSelected = false, Person = p });

            SubmitCommand = new RelayCommand(Submit);
        }

        private void Submit()
        {
            var selected = People.Where(p => p.IsSelected).Select(p => p.Person).ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show("Please select at least one person to copy.");
                return;
            }

            var dialog = new CommonOpenFileDialog { IsFolderPicker = true, Title = "เลือกโฟลเดอร์ปลายทาง" };
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            SelectedPath = dialog.FileName;
            SelectedPeople = selected;

            // ปิดหน้าต่างหลังสำเร็จ
            var window = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);

            if (window != null)
                window.DialogResult = true;
        }
    }

    public class PersonWrapper
    {
        public bool IsSelected { get; set; }
        public PersonModel Person { get; set; }
    }
}
