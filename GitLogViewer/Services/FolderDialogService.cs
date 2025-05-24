using GitLogViewer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;

namespace GitLogViewer.Services
{
    public class FolderDialogService : IFolderDialogService
    {
        public string OpenFolderDialog()
        {
            Debug.WriteLine("Folder dialog called");  // เพิ่มบรรทัดนี้
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "เลือกโฟลเดอร์"
            };

            return dialog.ShowDialog() == CommonFileDialogResult.Ok
                ? dialog.FileName
                : null;
        }
    }
}

