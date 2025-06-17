using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitLogViewer.Models
{
    public class FolderTargetModel
    {
        public bool IsSelected { get; set; }
        public string FolderName { get; set; }
        public string FullPath { get; set; }
    }

}
