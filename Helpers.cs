using System;
using System.IO;
using System.Linq;
using System.Windows.Threading;

namespace RaspCommander
{
    internal static class Helpers
    {

        internal const string Up = "..";

        internal static class VirtualFolders
        {
            public static string MyComputer = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            public static string Desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private static bool PathsAreDifferent(string from, string to)
            => !new DirectoryInfo(from).Parent.FullName.Equals(new DirectoryInfo(to).FullName, StringComparison.OrdinalIgnoreCase);

        private static bool CopyError(string path1, string path2)
            => string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2);

        internal static void Copy(string from, string to, Dispatcher dis = null, Progress progressWindow = null)
        {
            if (CopyError(from, to))
                throw new ArgumentException(Properties.Resources.EXC_CANNOT_COPY);

            if (File.GetAttributes(from).HasFlag(FileAttributes.Directory))
            {
                if (progressWindow == null && dis != null)
                {
                    dis.Invoke(() =>
                    {
                        progressWindow = new Progress((uint)Directory.GetFiles(from, "*.*", SearchOption.AllDirectories).Length);
                        progressWindow.Show();
                    });
                }

                var destination = Path.Combine(to, Path.GetFileName(from));

                if (!PathsAreDifferent(from, to))
                    throw new ArgumentException(Properties.Resources.EXC_SAME_PATH);
                else
                {
                    Directory.CreateDirectory(destination);

                    foreach (var path in Directory.GetDirectories(from).Concat(Directory.GetFiles(from)))
                        Copy(path, destination, dis, progressWindow);
                }
            }
            else
            {
                if (progressWindow == null)
                {
                    dis?.Invoke(() =>
                    {
                        progressWindow = new Progress(1);
                        progressWindow.Show();
                    });
                }

                var destination = Path.Combine(to, Path.GetFileName(from));

                if (!PathsAreDifferent(from, to))
                    throw new ArgumentException(Properties.Resources.EXC_ALREADY_EXISTS);
                else
                {
                    dis?.Invoke(() =>
                    {
                        progressWindow.ProgressText = from;
                    });

                    File.Copy(from, destination);

                    dis?.Invoke(() =>
                    {
                        progressWindow.ProgressValue++;
                    });
                }
            }
        }

    }
}
