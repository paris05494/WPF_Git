using GitLogViewer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GitLogViewer.Services
{
    public class GitService
    {
        //ดึง Git log 10 รายการล่าสุด
        public List<GitLogEntry> GetGitLogs(string repoPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "log -n 10 --pretty=format:\"%H|%an|%ad|%s\" --date=iso",
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Split('\n')
                         .Where(line => !string.IsNullOrWhiteSpace(line))
                         .Select(line =>
                         {
                             var parts = line.Split('|');
                             return new GitLogEntry
                             {
                                 Hash = parts[0],
                                 Author = parts[1],
                                 Date = parts[2],
                                 Message = parts[3]
                             };
                         })
                         .ToList();
        }

        //ดึงรายการไฟล์ใน Git repository
        public List<string> GetFiles(string repoPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "ls-files",
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Split('\n')
                         .Where(line => !string.IsNullOrWhiteSpace(line))
                         .ToList();
        }
    }
}
