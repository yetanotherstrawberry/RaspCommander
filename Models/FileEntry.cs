using System;

namespace RaspCommander.Models
{
    public class FileEntry
    {

        public FileEntry(string name, string path, DateTime date)
        {
            Path = path;
            Name = name;
            Date = date;
        }

        public string Path { get; }
        public string Name { get; }
        public DateTime Date { get; }

    }
}
