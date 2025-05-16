using System;

namespace GitLogViewer.Models
{
    public class GitLogEntry
    {
        public string Hash { get; set; }
        public string Author { get; set; }
        public string Date { get; set; }
        public string Message { get; set; }
    }
}
