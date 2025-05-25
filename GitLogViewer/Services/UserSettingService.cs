using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GitLogViewer.Services
{
    public class UserSettingService
    {
        private static readonly string SettingFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GitLogViewer",
            "user_settings.txt"
        );

        public void SaveWorkPath(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingFilePath));
            File.WriteAllText(SettingFilePath, path ?? string.Empty);
        }

        public string LoadWorkPath()
        {
            return File.Exists(SettingFilePath)
                ? File.ReadAllText(SettingFilePath)
                : null;
        }
    }
}
