using RaspCommander.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RaspCommander
{
    public partial class MainWindow : Window
    {

        private Point Mouse;
        private string LeftCatalog, RightCatalog;

        public ObservableCollection<FileEntry> LeftEntries { get; } = new ObservableCollection<FileEntry>();
        public ObservableCollection<FileEntry> RightEntries { get; } = new ObservableCollection<FileEntry>();

        private void ShowError(Exception exc, bool? left = null)
        {
            MessageBox.Show(this, exc.Message, Properties.Resources.ERROR_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
            if (left != null) ChangeContent(left.Value, left.Value ? LeftCatalog : RightCatalog);
        }

        private void ChangeContent(bool left, string path, bool checkForSameDirs = true)
        {
            var dirToBeChanged = left ? LeftEntries : RightEntries;

            try
            {
                dirToBeChanged.Clear();

                if (Helpers.VirtualFolders.MyComputer.Equals(path))
                    foreach (var entry in Environment.GetLogicalDrives())
                        dirToBeChanged.Add(new FileEntry(entry, entry, Directory.GetCreationTime(entry)));
                else
                {
                    if (Directory.GetParent(path) != null)
                        dirToBeChanged.Add(new FileEntry(Helpers.Up, Directory.GetParent(path).ToString(), Directory.GetParent(path).CreationTime));
                    else
                        dirToBeChanged.Add(new FileEntry(Helpers.Up, Helpers.VirtualFolders.MyComputer, DateTime.Now));

                    var temp = new ObservableCollection<FileEntry>(
                        Directory.EnumerateFileSystemEntries(path)
                        .Select(tempPath => new FileEntry(Path.GetFileName(tempPath), tempPath, File.GetCreationTime(tempPath))));

                    foreach (var tempEntry in temp)
                        (left ? LeftEntries : RightEntries).Add(tempEntry);
                }

                if (left) LeftCatalog = path;
                else RightCatalog = path;

                if (checkForSameDirs && LeftCatalog.Equals(RightCatalog))
                    ChangeContent(!left, LeftCatalog, !checkForSameDirs);
            }
            catch (Exception exc)
            {
                ShowError(exc, left);
            }
        }

        private async void Copy(bool left, DragEventArgs dropArgs)
        {
            try
            {
                LeftGrid.IsEnabled = false;
                RightGrid.IsEnabled = false;
                var destPath = left ? LeftCatalog : RightCatalog;

                if (dropArgs.Data.GetDataPresent(typeof(FileEntry)))
                {
                    var sourcePath = (FileEntry)dropArgs.Data.GetData(typeof(FileEntry));
                    await Task.Factory.StartNew(() => Helpers.Copy(sourcePath.Path, destPath));
                }
                else if (dropArgs.Data.GetDataPresent(DataFormats.FileDrop))
                    foreach (var element in (string[])dropArgs.Data.GetData(DataFormats.FileDrop))
                        await Task.Factory.StartNew(() => Helpers.Copy(element, destPath));
                else
                    throw new NotSupportedException(Properties.Resources.EXC_FORMAT);

                ChangeContent(left, destPath);
            }
            catch (Exception exc)
            {
                ShowError(exc);
            }
            finally
            {
                LeftGrid.IsEnabled = true;
                RightGrid.IsEnabled = true;

                try
                {
                    ChangeContent(left, left ? LeftCatalog : RightCatalog);
                }
                catch (Exception innerExc)
                {
                    ShowError(innerExc);
                }
            }
        }

        private void NewFolder(bool left)
        {
            try
            {
                if (Helpers.VirtualFolders.MyComputer.Equals(left ? LeftCatalog : RightCatalog))
                    throw new InvalidOperationException(Properties.Resources.EXC_VIRTUAL_FOLDER_NEW_FOLDER);

                var modal = new NewFolder()
                {
                    Owner = this,
                };
                if ((!modal?.ShowDialog()) ?? true) return;
                var name = modal.Text;

                Directory.CreateDirectory(Path.Combine(left ? LeftCatalog : RightCatalog, name));

                ChangeContent(left, left ? LeftCatalog : RightCatalog);
            }
            catch (Exception exc)
            {
                ShowError(exc);
            }
        }

        private void Interaction(string path, MouseButtonEventArgs clickArgs, bool left)
        {
            try
            {
                if (MouseButton.Left.Equals(clickArgs.ChangedButton))
                {
                    if (Helpers.VirtualFolders.MyComputer.Equals(path) || File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                        ChangeContent(left, path);
                    else
                        Process.Start(path);
                }
            }
            catch (Exception exc)
            {
                ShowError(exc);
            }
        }

        private async void ContextMenuDelete_Click(object sender, bool left)
        {
            try
            {
                LeftGrid.IsEnabled = false;
                RightGrid.IsEnabled = false;

                if (!(sender is MenuItem item) || !(item.Tag is FileEntry obj))
                    throw new InvalidDataException(Properties.Resources.EXC_INVALID_DATA);

                if (Helpers.VirtualFolders.MyComputer.Equals(left ? LeftCatalog : RightCatalog) || Helpers.Up.Equals(obj.Name))
                    throw new InvalidOperationException(Properties.Resources.EXC_VIRTUAL_FOLDER_DELETE);

                if (File.GetAttributes(obj.Path).HasFlag(FileAttributes.Directory))
                    await Task.Factory.StartNew(() => Directory.Delete(obj.Path, true));
                else
                    await Task.Factory.StartNew(() => File.Delete(obj.Path));
            }
            catch (Exception exc)
            {
                ShowError(exc);
            }
            finally
            {
                LeftGrid.IsEnabled = true;
                RightGrid.IsEnabled = true;

                try
                {
                    ChangeContent(left, left ? LeftCatalog : RightCatalog);
                }
                catch (Exception innerExc)
                {
                    ShowError(innerExc);
                }
            }
        }

        private void KeyAction(object obj, KeyEventArgs keyArgs)
        {
            try
            {
                switch (keyArgs.Key)
                {
                    case Key.L:
                        MessageBox.Show(Properties.Resources.LICENSE, Properties.Resources.LICENSE_TITLE);
                        break;
                    case Key.F1:
                        MessageBox.Show(Encoding.UTF8.GetString(Properties.Resources.README), Properties.Resources.README_TITLE);
                        break;
                }
            }
            catch (Exception exc)
            {
                ShowError(exc);
            }
        }

        private void DragMove(object obj, MouseEventArgs clickArgs)
        {
            var move = clickArgs.GetPosition((DataGridRow)obj) - Mouse;

            if (
                MouseButtonState.Pressed.Equals(clickArgs.LeftButton) &&
                Math.Abs(move.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(move.Y) > SystemParameters.MinimumVerticalDragDistance
            )
            {
                var movedObj = (DataGridRow)obj;
                var data = (FileEntry)movedObj.Item;
                DragDrop.DoDragDrop(movedObj, data, DragDropEffects.Move);
            }
        }

        private void DragClick(object obj, MouseButtonEventArgs clickArgs)
            => Mouse = clickArgs.GetPosition((DataGridRow)obj);

        private void DataGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(FileEntry)) || e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
            else e.Effects = DragDropEffects.None;
        }

        private void LeftItemClick(object obj, MouseButtonEventArgs clickArgs)
            => Interaction(((FileEntry)((DataGridRow)obj).Item).Path, clickArgs, true);
        private void RightItemClick(object obj, MouseButtonEventArgs clickArgs)
            => Interaction(((FileEntry)((DataGridRow)obj).Item).Path, clickArgs, false);

        private void LeftMenuItemNewFolder_Click(object sender, RoutedEventArgs e) => NewFolder(true);
        private void RightMenuItemNewFolder_Click(object sender, RoutedEventArgs e) => NewFolder(false);

        private void LeftMenuItemRefresh_Click(object sender, RoutedEventArgs e) => ChangeContent(true, LeftCatalog);
        private void RightMenuItemRefresh_Click(object sender, RoutedEventArgs e) => ChangeContent(false, RightCatalog);

        private void LeftGrid_Drop(object sender, DragEventArgs e) => Copy(true, e);
        private void RightGrid_Drop(object sender, DragEventArgs e) => Copy(false, e);

        private void RightMenuItemDelete_Click(object sender, RoutedEventArgs e) => ContextMenuDelete_Click(sender, false);
        private void LeftMenuItemDelete_Click(object sender, RoutedEventArgs e) => ContextMenuDelete_Click(sender, true);

        public MainWindow()
        {
            ChangeContent(true, Helpers.VirtualFolders.MyComputer);
            ChangeContent(false, Helpers.VirtualFolders.Desktop);

            InitializeComponent();
        }

    }
}
